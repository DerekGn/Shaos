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
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(plugInFactory);
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
        public Instance AddInstance(
            int id,
            string name,
            string assemblyFileName)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(assemblyFileName);

            VerifyInstanceCount();

            var instance = _executingInstances.FirstOrDefault(_ => _.Id == id);

            if (instance == null)
            {
                _logger.LogInformation("Creating ExecutingInstance: [{Id}] Name: [{Name}]",
                    id,
                    name);

                instance = new Instance()
                {
                    Id = id,
                    Name = name,
                    State = InstanceState.None,
                    AssemblyFilePath = assemblyFileName
                };

                _executingInstances.Add(instance);

                InstanceStateChanged?.Invoke(this,
                    new InstanceStateEventArgs(instance.Id, instance.State));

                return instance;
            }
            else
            {
                throw new InstanceExistsException(id);
            }
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
                    throw new InstanceRunningException(id);
                }

                _executingInstances.Remove(instance);
            }
            else
            {
                throw new InstanceNotFoundException(id);
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
                throw new InstanceNotFoundException(id);
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
                _logger.LogWarning("Instance: [{Id}] Not Found", id);

                throw new InstanceNotFoundException(id);
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

        private bool LoadInstancePlugInFromAssembly(Instance instance)
        {
            try
            {
                instance.State = InstanceState.PlugInLoading;

                instance.UnloadingContext = new UnloadingWeakReference<IRuntimeAssemblyLoadContext>(
                    _runtimeAssemblyLoadContextFactory.Create(instance.AssemblyFilePath));
                instance.Assembly = instance.UnloadingContext.Target.LoadFromAssemblyPath(instance.AssemblyFilePath);
                instance.PlugIn = _plugInFactory.CreateInstance(instance.Assembly);

                instance.State = InstanceState.PlugInLoaded;

                InstanceStateChanged?.Invoke(this,
                    new InstanceStateEventArgs(instance.Id, instance.State));
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

            return instance.State == InstanceState.PlugInLoaded;
        }

        private void StartExecutingInstance(Instance instance)
        {
            _logger.LogInformation("Loading PlugIn from assembly Id: [{Id}] Name: [{Name}] AssemblyFilePath: [{AssemblyFilePath}]",
                    instance.Id,
                    instance.Name,
                    instance.AssemblyFilePath);

            if (LoadInstancePlugInFromAssembly(instance))
            {
                _logger.LogInformation("Starting PlugIn instance execution Id: [{Id}] Name: [{Name}]",
                    instance.Id,
                    instance.Name);

                StartInstanceExecution(instance);
            }
        }

        private void StartInstanceExecution(Instance instance)
        {
            try
            {
                instance.State = InstanceState.Activating;
                instance.TokenSource = new CancellationTokenSource();
                instance.Task = Task.Run(
                    async () => await instance.PlugIn!.ExecuteAsync(instance.TokenSource!.Token))
                    .ContinueWith((antecedent) => UpdateStateOnCompletion(instance, antecedent));
                instance.State = InstanceState.Running;

                InstanceStateChanged?.Invoke(this,
                    new InstanceStateEventArgs(instance.Id, instance.State));
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Exception occurred during instance execution start. Id: [{Id}] Name: [{Name}]",
                    instance.Id,
                    instance.Name);

                instance.State = InstanceState.ActivationFaulted;
                instance.Exception = exception;

                InstanceStateChanged?.Invoke(this,
                    new InstanceStateEventArgs(instance.Id, instance.State));
            }
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

                instance.CleanUp();
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