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
using System.Diagnostics;

namespace Shaos.Services.Runtime.Host
{
    /// <summary>
    /// An instance hosting
    /// </summary>
    /// <remarks>
    /// Responsible for the runtime hosting and management of <see cref="RuntimeInstance"/>
    /// </remarks>
    public partial class RuntimeInstanceHost : IRuntimeInstanceHost
    {
        internal readonly List<RuntimeInstance> _executingInstances;
        internal readonly Dictionary<int, RuntimeInstanceLoadContext> _instanceLoadContexts;

        private readonly ILogger<RuntimeInstanceHost> _logger;
        private readonly IOptions<RuntimeInstanceHostOptions> _options;
        private readonly IRuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;

        /// <summary>
        /// Create an <see cref="RuntimeInstance"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance</param>
        /// <param name="options">The <see cref="IOptions{TOptions}"/> of <see cref="RuntimeInstanceHostOptions"/></param>
        /// <param name="runtimeAssemblyLoadContextFactory">The <see cref="IRuntimeAssemblyLoadContextFactory"/> for loading <see cref="IRuntimeAssemblyLoadContext"/></param>
        public RuntimeInstanceHost(ILogger<RuntimeInstanceHost> logger,
                                   IOptions<RuntimeInstanceHostOptions> options,
                                   IRuntimeAssemblyLoadContextFactory runtimeAssemblyLoadContextFactory)
        {
            _logger = logger;
            _options = options;
            _runtimeAssemblyLoadContextFactory = runtimeAssemblyLoadContextFactory;

            _executingInstances = [];
            _instanceLoadContexts = [];
        }

        /// <inheritdoc/>
        public event EventHandler<RuntimeInstanceStateEventArgs>? InstanceStateChanged;

        /// <inheritdoc/>
        public IReadOnlyList<RuntimeInstance> Instances => _executingInstances.AsReadOnly();

        /// <inheritdoc/>
        public RuntimeInstance CreateInstance(int id,
                                              int plugInId,
                                              string instanceName,
                                              string assemblyPath,
                                              bool configurable = false)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(instanceName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(assemblyPath);

            VerifyInstanceCount();

            var instance = _executingInstances.FirstOrDefault(_ => _.Id == id);

            if (instance == null)
            {
                LogCreatingInstance(id,
                                     instanceName);

                instance = new RuntimeInstance(id,
                                               plugInId,
                                               instanceName,
                                               assemblyPath,
                                               configurable);

                _executingInstances.Add(instance);

                InitaliseInstanceLoadContext(instance);

                InstanceStateChanged?.Invoke(this,
                                             new RuntimeInstanceStateEventArgs(id, instance.State));

                return instance;
            }
            else
            {
                LogInstanceExists(id, instance.Name);

                throw new InstanceExistsException(id);
            }
        }

        /// <inheritdoc/>
        public RuntimeInstanceLoadContext GetInstanceLoadContext(int id)
        {
            var instance = _executingInstances.FirstOrDefault(_ => _.Id == id) ?? throw new InstanceNotFoundException(id);

            InitaliseInstanceLoadContext(instance);

            return _instanceLoadContexts[id];
        }

        /// <inheritdoc/>
        public bool InstanceExists(int id)
        {
            return _executingInstances.Any(_ => _.Id == id);
        }

        /// <inheritdoc/>
        public void RemoveInstance(int id)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

            ResolveExecutingInstance(id, (instance) =>
            {
                if (instance.State == RuntimeInstanceState.Running)
                {
                    LogInstanceAlreadyRunning(id,
                                              instance.Name);

                    throw new InstanceRunningException(id);
                }

                _executingInstances.Remove(instance);
            });
        }

        /// <inheritdoc/>
        public RuntimeInstance StartInstance(int id,
                                             IPlugIn plugIn)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
            ArgumentNullException.ThrowIfNull(plugIn);

            return ResolveExecutingInstance(id, (instance) =>
            {
                if (instance.State == RuntimeInstanceState.Running)
                {
                    LogInstanceAlreadyRunning(id,
                                              instance.Name);

                    throw new InstanceRunningException(id);
                }
                else
                {
                    instance.LoadContext(plugIn);

                    _ = Task.Run(() => StartExecutingInstance(instance));
                }

                return instance;
            });
        }

        /// <inheritdoc/>
        public RuntimeInstance StopInstance(int id)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

            return ResolveExecutingInstance(id, (instance) =>
            {
                LogStoppingInstance(id,
                                    instance.Name);

                if (instance.State != RuntimeInstanceState.Running)
                {
                    LogInstanceNotRunning(id,
                                          instance.Name);
                }
                else
                {
                    _ = Task.Run(async () => await StopExecutingInstanceAsync(instance));
                }

                return instance;
            });
        }

        internal void UpdateStateOnCompletion(RuntimeInstance instance,
                                              Task antecedent)
        {
            LogPlugInCompleted(Environment.NewLine,
                               antecedent.ToLoggingString());

            if (antecedent.Status == TaskStatus.RanToCompletion || antecedent.Status == TaskStatus.Canceled)
            {
                instance.SetComplete();

                LogPlugInInstanceCompleted(instance.Id,
                                           instance.Name,
                                           antecedent.Status);
            }
            else if (antecedent.Status == TaskStatus.Faulted)
            {
                instance.SetFaulted(antecedent.Exception);

                LogPlugInInstanceFaulted(antecedent.Exception,
                                         instance.Id,
                                         instance.Name,
                                         antecedent.Status);
            }

            LogUnloadingInstance(instance.Id,
                                 instance.Name);

            instance.ExecutionContext?.Dispose();

            if (!_executingInstances.Any(_ => _.State == RuntimeInstanceState.Running && _.PlugInId == instance.PlugInId))
            {
                UnloadInstanceLoadContext(instance);
            }

            InstanceStateChanged?.Invoke(this,
                    new RuntimeInstanceStateEventArgs(instance.Id, instance.State));
        }

        private async Task ExecutePlugInMethod(RuntimeInstance instance,
                                               CancellationToken cancellationToken = default)
        {
            instance.SetRunning();

            InstanceStateChanged?.Invoke(this,
                                         new RuntimeInstanceStateEventArgs(instance.Id, instance.State));

            try
            {
                await instance.ExecuteAsync(cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                LogPlugInTaskCancelled(exception);
            }
            catch (Exception exception)
            {
                LogUnhandledException(exception);

                instance.SetFaulted(exception);
            }
        }

        private void InitaliseInstanceLoadContext(RuntimeInstance instance)
        {
            var instanceLoadContext = _instanceLoadContexts.GetValueOrDefault(instance.PlugInId);

            if (instanceLoadContext == null)
            {
                instanceLoadContext = new RuntimeInstanceLoadContext(_runtimeAssemblyLoadContextFactory.Create(instance.AssemblyPath));
                _instanceLoadContexts.Add(instance.PlugInId, instanceLoadContext);
            }
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Creating Runtime Instance for Device [{id}] Name: [{name}]")]
        private partial void LogCreatingInstance(int id,
                                                 string name);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Runtime execution instance count exceeded. Count: [{count}] Max: [{max}]")]
        private partial void LogExecutionInstanceCount(int count,
                                                       int max);

        [LoggerMessage(Level = LogLevel.Information, Message = "Runtime execution instance Count: [{count}] Max: [{max}]")]
        private partial void LogExecutionInstanceCountExceeded(int count,
                                                               int max);

        [LoggerMessage(Level = LogLevel.Error, Message = "Runtime Instance Id: [{Id}] Name: [{name}] is already running")]
        private partial void LogInstanceAlreadyRunning(int id,
                                                       string name);

        [LoggerMessage(Level = LogLevel.Error, Message = "Runtime Instance Id: [{id}] Name: [{name}] exists")]
        private partial void LogInstanceExists(int id,
                                               string name);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Runtime instance: [{id}] Name: [{name}] Not Running")]
        private partial void LogInstanceNotRunning(int id,
                                                   string name);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Runtime Instance not stopped within timeout. Id: [{id}] Name: [{name}]")]
        private partial void LogInstanceNotStoppedWithinTimeOut(int id,
                                                                string name);

        [LoggerMessage(Level = LogLevel.Information, Message = "Stopped execution. Id: [{id}] Name: [{name}]")]
        private partial void LogInstanceStopped(int id,
                                                string name);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Completed PlugIn Runtime Instance Task: {newLine}{task}")]
        private partial void LogPlugInCompleted(string newLine,
                                                string task);

        [LoggerMessage(Level = LogLevel.Information, Message = "Instance completed. Id: [{id}] Name: [{name}] Task Status: [{status}]")]
        private partial void LogPlugInInstanceCompleted(int id,
                                                        string name,
                                                        TaskStatus status);

        [LoggerMessage(Level = LogLevel.Error, Message = "Instance completed. Id: [{id}] Name: [{name}] Task Status: [{status}]")]
        private partial void LogPlugInInstanceFaulted(AggregateException? exception,
                                                      int id,
                                                      string name,
                                                      TaskStatus status);

        [LoggerMessage(Level = LogLevel.Warning, Message = "PlugIn Task cancelled")]
        private partial void LogPlugInTaskCancelled(OperationCanceledException exception);

        [LoggerMessage(Level = LogLevel.Information, Message = "Starting PlugIn Runtime Instance execution Id: [{id}] Name: [{name}]")]
        private partial void LogStartingInstance(int id,
                                                 string name);

        [LoggerMessage(Level = LogLevel.Information, Message = "Stopping Runtime Instance: [{id}] Name: [{name}]")]
        private partial void LogStoppingInstance(int id,
                                                 string name);

        [LoggerMessage(Level = LogLevel.Error, Message = "Unable to find Runtime Instance Id: [{id}]")]
        private partial void LogUnableToFindInstance(int id);

        [LoggerMessage(Level = LogLevel.Critical, Message = "An unhandled exception occurred in PlugIn Runtime Instance")]
        private partial void LogUnhandledException(Exception exception);

        [LoggerMessage(Level = LogLevel.Information, Message = "Unloading instance execution context for instance: [{id}] Name: [{name}]")]
        private partial void LogUnloadingInstance(int id,
                                                  string name);

        [LoggerMessage(Level = LogLevel.Information, Message = "Unloading instance context for PlugIn: [{plugInId}]")]
        private partial void LogUnloadingInstance(int plugInId);

        [DebuggerStepThrough]
        private T ResolveExecutingInstance<T>(int id,
                                              Func<RuntimeInstance, T> operation)
        {
            var instance = _executingInstances
                .FirstOrDefault(_ => _.Id == id);

            if (instance == null)
            {
                LogUnableToFindInstance(id);

                throw new InstanceNotFoundException(id);
            }
            else
            {
                return operation(instance);
            }
        }

        [DebuggerStepThrough]
        private void ResolveExecutingInstance(int id,
                                              Action<RuntimeInstance> operation)
        {
            var instance = _executingInstances
                .FirstOrDefault(_ => _.Id == id);

            if (instance == null)
            {
                LogUnableToFindInstance(id);

                throw new InstanceNotFoundException(id);
            }
            else
            {
                operation(instance);
            }
        }

        private void StartExecutingInstance(RuntimeInstance instance)
        {
            LogStartingInstance(instance.Id,
                                instance.Name);

            instance.StartExecution(async (cancellationToken) => await ExecutePlugInMethod(instance, cancellationToken),
                                    (antecedent) => UpdateStateOnCompletion(instance, antecedent));

            InstanceStateChanged?.Invoke(this,
                new RuntimeInstanceStateEventArgs(instance.Id, instance.State));
        }

        private async Task StopExecutingInstanceAsync(RuntimeInstance instance)
        {
            LogStoppingInstance(instance.Id, instance.Name);

            if (await instance.StopExecutionAsync(_options.Value.TaskStopTimeout))
            {
                LogInstanceStopped(instance.Id,
                                   instance.Name);
            }
            else
            {
                LogInstanceNotStoppedWithinTimeOut(instance.Id,
                                                   instance.Name);
            }
        }

        private void UnloadInstanceLoadContext(RuntimeInstance instance)
        {
            LogUnloadingInstance(instance.PlugInId);

            var instanceLoadContext = _instanceLoadContexts.GetValueOrDefault(instance.PlugInId);

            if (instanceLoadContext != null)
            {
                instanceLoadContext.Dispose();

                _instanceLoadContexts.Remove(instance.PlugInId);
            }
        }

        private void VerifyInstanceCount()
        {
            if (_executingInstances.Count == _options.Value.MaxExecutingInstances)
            {
                LogExecutionInstanceCountExceeded(_executingInstances.Count,
                                                  _options.Value.MaxExecutingInstances);

                throw new MaxInstancesRunningException(_executingInstances.Count);
            }
            else
            {
                LogExecutionInstanceCount(_executingInstances.Count,
                                          _options.Value.MaxExecutingInstances);
            }
        }
    }
}