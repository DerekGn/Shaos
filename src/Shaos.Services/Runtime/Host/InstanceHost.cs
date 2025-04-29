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
using Shaos.Sdk;
using Shaos.Services.Extensions;
using Shaos.Services.Runtime.Exceptions;

namespace Shaos.Services.Runtime.Host
{
    public class InstanceHost : IInstanceHost
    {
        internal readonly List<Instance> _executingInstances;
        private readonly ILogger<InstanceHost> _logger;
        private readonly IOptions<InstanceHostOptions> _options;
        private readonly IRuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;

        public InstanceHost(
            ILogger<InstanceHost> logger,
            IOptions<InstanceHostOptions> options,
            IRuntimeAssemblyLoadContextFactory runtimeAssemblyLoadContextFactory)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(runtimeAssemblyLoadContextFactory);

            _logger = logger;
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
            string assemblyFilePath)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(assemblyFilePath);

            VerifyInstanceCount();

            var instance = _executingInstances
                .FirstOrDefault(_ => _.Id == id);

            if (instance == null)
            {
                _logger.LogInformation("Creating Instance: [{Id}] Name: [{Name}]", id, name);

                instance = InitialiseNewInstance(id, name, assemblyFilePath);

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
        public Instance InitialiseInstance(
            int id,
            IPlugIn plugIn,
            object? configuration = default)
        {
            ArgumentNullException.ThrowIfNull(plugIn);

            var instance = _executingInstances
                .FirstOrDefault(_ => _.Id == id);

            if (instance != null)
            {
                _logger.LogDebug("Initialising instance Id: [{Id}]", id);

                instance.PlugIn = plugIn;
                instance.Configuration = configuration;
                instance.State = InstanceState.PlugInLoaded;

                InstanceStateChanged?.Invoke(this,
                    new InstanceStateEventArgs(instance.Id, instance.State));
            }
            else
            {
                HandleInstanceNotFound(id);
            }

            return instance;
        }

        // </inheritdoc>
        public bool InstanceExists(int id)
        {
            return _executingInstances.Any(_ => _.Id == id);
        }

        // </inheritdoc>
        public void RemoveInstance(int id)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

            var instance = _executingInstances.FirstOrDefault(_ => _.Id == id);

            if (instance != null)
            {
                if (instance.State == InstanceState.Running)
                {
                    _logger.LogError("Instance Id: [{Id}] is already running", id);

                    throw new InstanceRunningException(id);
                }

                _executingInstances.Remove(instance);
            }
            else
            {
                HandleInstanceNotFound(id);
            }
        }

        // </inheritdoc>
        public Instance StartInstance(int id)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

            var instance = _executingInstances.FirstOrDefault(_ => _.Id == id);

            if (instance != null)
            {
                if (instance.State != InstanceState.Running)
                {
                    if (instance.State == InstanceState.AwaitingInitialisation)
                    {
                        _logger.LogError("PlugIn: [{Id}] Name: [{Name}] not loaded",
                            instance.Id,
                            instance.Name);

                        throw new InstanceNotInitialisedException();
                    }

                    instance.StartTime = DateTime.UtcNow;

                    _ = Task.Run(() => StartExecutingInstance(instance));
                }
                else
                {
                    _logger.LogWarning("PlugIn: [{Id}] Name: [{Name}] Already Running",
                        instance.Id,
                        instance.Name);
                }
            }
            else
            {
                HandleInstanceNotFound(id);
            }

            return instance;
        }

        // </inheritdoc>
        public Instance StopInstance(int id)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

            var instance = _executingInstances.FirstOrDefault(_ => _.Id == id);

            if (instance == null)
            {
                HandleInstanceNotFound(id);
            }
            else
            {
                if (instance.State != InstanceState.Running)
                {
                    _logger.LogWarning("Instance: [{Id}] Name: [{Name}] Not Running",
                        instance.Id,
                        instance.Name);
                }
                else
                {
                    instance.StopTime = DateTime.UtcNow;
                    _ = Task.Run(async () => await StopExecutingInstanceAsync(instance));
                }
            }

            return instance;
        }

        private async Task ExecutePlugInMethod(Instance instance)
        {
            instance.State = InstanceState.Running;
            InstanceStateChanged?.Invoke(this,
                new InstanceStateEventArgs(instance.Id, instance.State));

            await instance.PlugIn!.ExecuteAsync(instance.TokenSource!.Token);
        }

        private void HandleInstanceNotFound(int id)
        {
            _logger.LogError("Unable to find instance Id: [{Id}]", id);

            throw new InstanceNotFoundException(id);
        }

        private Instance InitialiseNewInstance(
            int id,
            string name,
            string assemblyFilePath)
        {
            var instance = new Instance()
            {
                Id = id,
                Name = name,
                State = InstanceState.None,
                AssemblyFilePath = assemblyFilePath
            };

            try
            {
                instance.Context = new UnloadingWeakReference<IRuntimeAssemblyLoadContext>(
                    _runtimeAssemblyLoadContextFactory.Create(instance.AssemblyFilePath));
                instance.Assembly = instance.Context.Target.LoadFromAssemblyPath(instance.AssemblyFilePath);
                instance.State = InstanceState.AwaitingInitialisation;
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Exception occurred during PlugIn load and create from Assembly. Id: [{Id}] Name: [{Name}]",
                    instance.Id,
                    instance.Name);

                instance.State = InstanceState.PlugInLoadFailure;
                instance.Exception = exception;

                InstanceStateChanged?.Invoke(this,
                    new InstanceStateEventArgs(instance.Id, instance.State));
            }

            return instance;
        }

        private void StartExecutingInstance(Instance instance)
        {
            _logger.LogInformation("Starting PlugIn instance execution Id: [{Id}] Name: [{Name}]",
                instance.Id,
                instance.Name);

            instance.State = InstanceState.Starting;
            instance.TokenSource = new CancellationTokenSource();
            instance.Task = Task.Run(
                async () => await ExecutePlugInMethod(instance))
                .ContinueWith((antecedent) => UpdateStateOnCompletion(instance, antecedent));

            InstanceStateChanged?.Invoke(this,
                new InstanceStateEventArgs(instance.Id, instance.State));
        }

        private async Task StopExecutingInstanceAsync(Instance instance)
        {
            _logger.LogInformation("Stopping Executing PlugInInstance: [{Id}] Name: [{Name}]",
                instance.Id,
                instance.Name);

            await instance.TokenSource!.CancelAsync();

            if (await Task.WhenAny(instance.Task!, Task.Delay(_options.Value.TaskStopTimeout)) == instance.Task)
            {
                _logger.LogInformation("Stopped execution. Id: [{Id}] Name: [{Name}]",
                    instance.Id,
                    instance.Name);

                instance.Dispose();
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

            instance.StartTime = DateTime.Now;

            if (antecedent.Status == TaskStatus.RanToCompletion || antecedent.Status == TaskStatus.Canceled)
            {
                instance.State = InstanceState.Complete;

                _logger.LogInformation("Instance completed. Id: [{Id}] Name: [{Name}] Task Status: [{Status}]",
                    instance.Id,
                    instance.Name,
                    antecedent.Status);
            }
            else if (antecedent.Status == TaskStatus.Faulted)
            {
                instance.State = InstanceState.Faulted;
                instance.Exception = antecedent.Exception;

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

                throw new RuntimeMaxInstancesRunningException();
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