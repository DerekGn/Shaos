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
using Shaos.Services.Json;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Factories;
using System.Diagnostics;

namespace Shaos.Services.Runtime.Host
{
    public class InstanceHost : IInstanceHost
    {
        internal readonly List<Instance> _executingInstances;
        internal readonly Dictionary<int, InstanceLoadContext> _instanceLoadContexts;

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
            _instanceLoadContexts = new Dictionary<int, InstanceLoadContext>();
        }

        // </inheritdoc>
        public event EventHandler<InstanceStateEventArgs> InstanceStateChanged;

        // </inheritdoc>
        public IReadOnlyList<Instance> Instances => _executingInstances.AsReadOnly();

        // </inheritdoc>
        public Instance CreateInstance(int id,
                                       int plugInId,
                                       string instanceName,
                                       string assemblyPath,
                                       InstanceConfiguration configuration)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(instanceName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(assemblyPath);
            ArgumentNullException.ThrowIfNull(configuration);

            VerifyInstanceCount();

            var instance = _executingInstances.FirstOrDefault(_ => _.Id == id);

            if (instance == null)
            {
                _logger.LogDebug("Creating load context for Instance PlugInId: [{Id}] AssemblyPath: [{AssemblyPath}]", id, assemblyPath);

                if (!_instanceLoadContexts.Any(_ => _.Key == plugInId))
                {
                    _instanceLoadContexts
                        .Add(plugInId, new InstanceLoadContext(_runtimeAssemblyLoadContextFactory.Create(assemblyPath)));
                }

                _logger.LogInformation("Creating Instance Id: [{Id}] Name: [{Name}]", id, instanceName);

                instance = new Instance(id, plugInId, instanceName, configuration);

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

        public object? LoadConfiguration(int id)
        {
            return ResolveExecutingInstance(id, (instance) =>
            {
                var loadContext = _instanceLoadContexts[instance.PlugInId];
                object? configuration = _plugInFactory.LoadConfiguration(loadContext.Assembly!);

                if(configuration != null)
                {
                    if (instance.Configuration.IsConfigured)
                    {
                        _logger.LogInformation("Updating the instance configuration. Id: [{Id}] Name: [{Name}]", instance.Id, instance.Name);

                        configuration = Utf8JsonSerilizer.Deserialize(instance.Configuration.Configuration!, configuration.GetType());
                    }
                }
                else
                {
#warning TODO
                    //throw new 
                }

                return configuration;
            });
        }

        // </inheritdoc>
        public void RemoveInstance(int id)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

            ResolveExecutingInstance(id, (instance) =>
            {
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
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

            return ResolveExecutingInstance(id, (instance) =>
            {
                if (instance.State == InstanceState.Running)
                {
                    _logger.LogWarning("Instance: [{Id}] Name: [{Name}] Already Running",
                                       instance.Id,
                                       instance.Name);
                }
                else if (instance.Configuration.RequiresConfiguration && !instance.Configuration.IsConfigured)
                {
                    _logger.LogError("Instance: [{Id}] Name: [{Name}] Not Configured",
                                     instance.Id,
                                     instance.Name);

                    throw new InstanceNotConfiguredException(id);
                }
                else
                {
                    var loadContext = _instanceLoadContexts[instance.PlugInId];

                    if (loadContext == null)
                    {
                        _logger.LogError("Instance: [{Id}] Name: [{Name}] PlugInId: [{PlugInId}] load context not found",
                                         instance.Id,
                                         instance.Name,
                                         instance.PlugInId);

                        throw new InstanceLoadContextNotFoundException(id);
                    }
                    else
                    {
                        object? configuration = null;

                        //if (instance.Configuration.RequiresConfiguration)
                        //{
                        //    _logger.LogInformation("Loading configuration for Instance: [{Id}] Name: [{Name}]", instance.Id, instance.Name);

                        //    configuration = _plugInFactory.LoadConfiguration(loadContext.Assembly!);

                        //    configuration = Utf8JsonSerilizer.Deserialize(instance.Configuration.Configuration!, configuration!.GetType());
                        //}

                        var plugIn = _plugInFactory.CreateInstance(loadContext.Assembly!, configuration);

                        if (plugIn == null)
                        {
                            _logger.LogError("Instance: [{Id}] Name: [{Name}] PlugInId: [{PlugInId}] PlugIn load failure",
                                             instance.Id,
                                             instance.Name,
                                             instance.PlugInId);

                            throw new PlugInNotCreatedException();
                        }
                        else
                        {
                            instance.LoadContext(plugIn!);

                            _ = Task.Run(() => StartExecutingInstance(instance));
                        }
                    }
                }

                return instance;
            });
        }

        // </inheritdoc>
        public Instance StopInstance(int id)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

            return ResolveExecutingInstance(id, (instance) =>
            {
                _logger.LogInformation("Stopping PlugIn instance Id: [{Id}] Name: [{Name}]", instance.Id, instance.Name);

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

                return instance;
            });
        }

        private async Task ExecutePlugInMethod(Instance instance,
                                               CancellationToken cancellationToken = default)
        {
            instance.SetRunning();

            InstanceStateChanged?.Invoke(this,
                new InstanceStateEventArgs(instance.Id, instance.State));

            try
            {
                await instance.ExecuteAsync(cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                _logger.LogWarning(exception, "PlugIn Task cancelled");
            }
            catch(Exception exception)
            {
                _logger.LogCritical(exception, "An un-handled exception occurred in PlugIn");

                instance.SetFaulted(exception);
            }
        }

        [DebuggerStepThrough]
        private T ResolveExecutingInstance<T>(int id,
                                              Func<Instance, T> operation)
        {
            var instance = _executingInstances
                .FirstOrDefault(_ => _.Id == id);

            if (instance == null)
            {
                _logger.LogError("Unable to find instance Id: [{Id}]", id);

                throw new InstanceNotFoundException(id);
            }
            else
            {
                return operation(instance);
            }
        }

        [DebuggerStepThrough]
        private void ResolveExecutingInstance(int id,
                                              Action<Instance> operation)
        {
            var instance = _executingInstances
                .FirstOrDefault(_ => _.Id == id);

            if (instance == null)
            {
                _logger.LogError("Unable to find instance Id: [{Id}]", id);

                throw new InstanceNotFoundException(id);
            }
            else
            {
                operation(instance);
            }
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

        private void UnloadInstanceLoadContext(Instance instance)
        {
            _logger.LogInformation("Unloading instance context for PlugIn: [{PlugInId}]", instance.PlugInId);

            var instanceLoadContext = _instanceLoadContexts.First(_ => _.Key == instance.PlugInId);

            instanceLoadContext.Value.Dispose();

            _instanceLoadContexts.Remove(instance.Id);
        }

        private void UpdateStateOnCompletion(Instance instance,
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

            _logger.LogInformation("Unloading instance execution context for instance: [{Id}] Name: [{Name}]", instance.Id, instance.Name);

            instance.Context?.Dispose();

            if (!_executingInstances.Any(_ => _.State == InstanceState.Running && _.PlugInId == instance.PlugInId))
            {
                UnloadInstanceLoadContext(instance);
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

                throw new MaxInstancesRunningException(_executingInstances.Count);
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