/*
* MIT License
*
* Copyright (c) 2025 Derek Goslin https://github.com/DerekGn
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shaos.Services.Extensions;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Factories;

namespace Shaos.Services.Runtime.Host
{
    public class InstanceHost : IInstanceHost
    {
        internal readonly List<Instance> _executingInstances;
        private readonly ILogger<InstanceHost> _logger;
        private readonly IOptions<InstanceHostOptions> _options;
        private readonly IPlugInFactory _plugInFactory;
        private readonly IRuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;

        public InstanceHost(
            ILogger<InstanceHost> logger,
            IPlugInFactory plugInFactory,
            IOptions<InstanceHostOptions> options,
            IRuntimeAssemblyLoadContextFactory runtimeAssemblyLoadContextFactory)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(plugInFactory);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(runtimeAssemblyLoadContextFactory);

            _logger = logger;
            _plugInFactory = plugInFactory;
            _options = options;
            _runtimeAssemblyLoadContextFactory = runtimeAssemblyLoadContextFactory;

            _executingInstances = new List<Instance>();
        }

        // </inheritdoc>
        public event EventHandler<InstanceStateEventArgs> InstanceStateChanged;

        // </inheritdoc>
        public IReadOnlyList<Instance> Instances => _executingInstances.AsReadOnly();

        // </inheritdoc>
        public Instance CreateInstance(
            int id,
            string name,
            string assemblyPath)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(assemblyPath);

            VerifyInstanceCount();

            var instance = _executingInstances.FirstOrDefault(_ => _.Id == id);

            if (instance == null)
            {
                _logger.LogInformation("Creating Instance Id: [{Id}] Name: [{Name}] Assembly: [{AssemblyPath}]",
                    id,
                    name,
                    assemblyPath);

                instance = new Instance(id, name, assemblyPath);

                _executingInstances.Add(instance);

                InstanceStateChanged?.Invoke(this,
                    new InstanceStateEventArgs(instance.Id, instance.State));

                return instance;
            }
            else
            {
                _logger.LogError("Instance Id: [{Id}] exists", id);

                throw new InstanceExistsException(id);
            }
        }

        // </inheritdoc>
        public bool InstanceExists(int id)
        {
            return _executingInstances.Any(_ => _.Id == id);
        }

        // </inheritdoc>
        public Instance LoadInstance(int id)
        {
            return ExecuteOperationOnInstance(id, (instance) =>
            {
                if (instance.State != InstanceState.None)
                {
                    _logger.LogWarning("Instance: [{Id}] Name: [{Name}] State: [{State}] already Loaded",
                        instance.Id,
                        instance.Name,
                        instance.State);
                }
                else
                {
                    _logger.LogInformation("Instance: [{Id}] Name: [{Name}] Loading",
                        instance.Id,
                        instance.Name);

                    if(!LoadInstanceContext(instance))
                    {
                        _logger.LogError(
                            instance.Exception,
                            "Instance: [{Id}] Name: [{Name}] Loading Failed",
                            instance.Id,
                            instance.Name);
                    }

                    InstanceStateChanged?.Invoke(this,
                            new InstanceStateEventArgs(instance.Id, instance.State));
                }
            });
        }

        // </inheritdoc>
        public void RemoveInstance(int id)
        {
            ExecuteOperationOnInstance(id, (instance) =>
            {
#warning TODO
                if (instance.State == InstanceState.Running)
                {
                    _logger.LogError("Instance Id: [{Id}] is already running", id);

                    throw new InstanceRunningException(id);
                }

                _executingInstances.Remove(instance);
            });
        }

        // </inheritdoc>
        public Instance StartInstance(int id)
        {
            return ExecuteOperationOnInstance(id, (instance) =>
            {
#warning TODO
                if (instance.State != InstanceState.Loaded)
                {
#warning TODO
                    //LoadInstanceContext(instance, configuration);

                    _ = Task.Run(() => StartExecutingInstance(instance));
                }
                else
                {
                    _logger.LogWarning("PlugIn: [{Id}] Name: [{Name}] State: [{State}] already Running",
                        instance.Id,
                        instance.Name,
                        instance.State);
                }
            });
        }

        // </inheritdoc>
        public Instance StopInstance(int id)
        {
            return ExecuteOperationOnInstance(id, (instance) =>
            {
                if (instance.State != InstanceState.Running)
                {
                    _logger.LogWarning("Instance: [{Id}] Name: [{Name}] Not Running",
                        instance.Id,
                        instance.Name);
                }
                else
                {
                    _ = Task.Run(async () => await StopExecutingInstanceAsync(instance));
                }
            });
        }

        // </inheritdoc>
        public Instance UnloadInstance(int id)
        {
            return ExecuteOperationOnInstance(id, (instance) =>
            {
                if (!InstanceUnloadable(instance))
                {
                    _logger.LogWarning("Instance: [{Id}] Name: [{Name}] Not In Loaded State",
                        instance.Id,
                        instance.Name);
                }
                else
                {
                    _logger.LogInformation("Instance: [{Id}] Name: [{Name}] Unloading",
                        instance.Id,
                        instance.Name);

                    instance.UnloadContext();
                }
            });
        }

        private static bool InstanceUnloadable(Instance instance)
        {
            return instance.State == InstanceState.Complete || instance.State == InstanceState.Faulted || instance.State == InstanceState.Loaded;
        }

        private Instance ExecuteOperationOnInstance(int id, Action<Instance> operation)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

            var instance = _executingInstances.FirstOrDefault(_ => _.Id == id);

            if (instance != null)
            {
                operation(instance);
            }
            else
            {
                HandleInstanceNotFound(id);
            }

            return instance;
        }

        private async Task ExecutePlugInMethod(Instance instance, CancellationToken cancellationToken = default)
        {
            InstanceStateChanged?.Invoke(this,
                new InstanceStateEventArgs(instance.Id, instance.State));

            await instance.ExecuteAsync(cancellationToken);
        }

        private void HandleInstanceNotFound(int id)
        {
            _logger.LogError("Unable to find instance Id: [{Id}]", id);

            throw new InstanceNotFoundException(id);
        }

        private bool LoadInstanceContext(Instance instance)
        {
            try
            {
                var assemblyLoadContext = _runtimeAssemblyLoadContextFactory.Create(instance.AssemblyPath);
                var assembly = assemblyLoadContext.LoadFromAssemblyPath(instance.AssemblyPath);
#warning TODO check null
                _plugInFactory.CreateInstance(assembly, out var plugIn, out var configuration);

                instance.LoadContext(plugIn, assemblyLoadContext, configuration);
            }
            catch (Exception exception)
            {
                instance.SetFaulted(exception);
            }

            return instance.State != InstanceState.Faulted;
        }

        private void StartExecutingInstance(Instance instance)
        {
            _logger.LogInformation("Starting PlugIn instance execution Id: [{Id}] Name: [{Name}]",
                instance.Id,
                instance.Name);

            instance.StartExecution(
                async (cancellationToken) => await ExecutePlugInMethod(instance, cancellationToken),
                (antecedent) => UpdateStateOnCompletion(instance, antecedent));

            InstanceStateChanged?.Invoke(this,
                new InstanceStateEventArgs(instance.Id, instance.State));
        }

        private async Task StopExecutingInstanceAsync(Instance instance)
        {
            _logger.LogInformation("Stopping Executing PlugInInstance: [{Id}] Name: [{Name}]",
                instance.Id,
                instance.Name);

            if (await instance.StopExecutionAsync(_options.Value.TaskStopTimeout))
            {
                _logger.LogInformation("Stopped execution. Id: [{Id}] Name: [{Name}]",
                    instance.Id,
                    instance.Name);
            }
            else
            {
                _logger.LogWarning("Instance not stopped within timeout. Id: [{Id}] Name: [{Name}]",
                    instance.Id,
                    instance.Name);
            }
        }

        private void UpdateStateOnCompletion(
            Instance instance,
            Task antecedent)
        {
            _logger.LogDebug("Completed PlugIn Task: {NewLine}{Task}",
                Environment.NewLine,
                antecedent.ToLoggingString());

            if (antecedent.Status == TaskStatus.RanToCompletion || antecedent.Status == TaskStatus.Canceled)
            {
                instance.SetComplete();

                _logger.LogInformation("Instance completed. Id: [{Id}] Name: [{Name}] Task Status: [{Status}]",
                    instance.Id,
                    instance.Name,
                    antecedent.Status);
            }
            else if (antecedent.Status == TaskStatus.Faulted)
            {
                instance.SetFaulted(antecedent.Exception);

                _logger.LogError(antecedent.Exception, "Instance completed. Id: [{Id}] Name: [{Name}] Task Status: [{Status}]",
                    instance.Id,
                    instance.Name,
                    antecedent.Status);
            }

            InstanceStateChanged?.Invoke(this,
                    new InstanceStateEventArgs(instance.Id, instance.State));
        }

        private void VerifyInstanceCount()
        {
            if (_executingInstances.Count == _options.Value.MaxExecutingInstances)
            {
                _logger.LogWarning("Execution Instance count exceeded. Count: [{Count}] Max: [{Max}]",
                    _executingInstances.Count,
                    _options.Value.MaxExecutingInstances);

                throw new MaxInstancesRunningException();
            }
            else
            {
                _logger.LogInformation("Execution Instance Count: [{Count}] Max: [{Max}]",
                    _executingInstances.Count,
                    _options.Value.MaxExecutingInstances);
            }
        }
    }
}