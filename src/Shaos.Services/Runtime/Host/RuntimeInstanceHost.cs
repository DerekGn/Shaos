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
    public class RuntimeInstanceHost : IRuntimeInstanceHost
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
                _logger.LogCreatingInstance(id,
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
                _logger.LogInstanceExists(id,
                                          instance.Name);

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
                    _logger.LogInstanceAlreadyRunning(id,
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
                    _logger.LogInstanceAlreadyRunning(id,
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
                _logger.LogStoppingInstance(id,
                                            instance.Name);

                if (instance.State != RuntimeInstanceState.Running)
                {
                    _logger.LogInstanceNotRunning(id,
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
            _logger.LogPlugInCompleted(Environment.NewLine,
                                       antecedent.ToLoggingString());

            if (antecedent.Status == TaskStatus.RanToCompletion || antecedent.Status == TaskStatus.Canceled)
            {
                instance.SetComplete();

                _logger.LogPlugInInstanceCompleted(instance.Id,
                                                   instance.Name,
                                                   antecedent.Status);
            }
            else if (antecedent.Status == TaskStatus.Faulted)
            {
                instance.SetFaulted(antecedent.Exception);

                _logger.LogPlugInInstanceFaulted(antecedent.Exception,
                                                 instance.Id,
                                                 instance.Name,
                                                 antecedent.Status);
            }

            _logger.LogUnloadingInstance(instance.Id,
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
                _logger.LogPlugInTaskCancelled(exception);
            }
            catch (Exception exception)
            {
                _logger.LogUnhandledException(exception);

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

        [DebuggerStepThrough]
        private T ResolveExecutingInstance<T>(int id,
                                              Func<RuntimeInstance, T> operation)
        {
            var instance = _executingInstances
                .FirstOrDefault(_ => _.Id == id);

            if (instance == null)
            {
                _logger.LogUnableToFindInstance(id);

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
                _logger.LogUnableToFindInstance(id);

                throw new InstanceNotFoundException(id);
            }
            else
            {
                operation(instance);
            }
        }

        private void StartExecutingInstance(RuntimeInstance instance)
        {
            _logger.LogStartingInstance(instance.Id,
                                        instance.Name);

            instance.StartExecution(async (cancellationToken) => await ExecutePlugInMethod(instance, cancellationToken),
                                    (antecedent) => UpdateStateOnCompletion(instance, antecedent));

            InstanceStateChanged?.Invoke(this,
                new RuntimeInstanceStateEventArgs(instance.Id, instance.State));
        }

        private async Task StopExecutingInstanceAsync(RuntimeInstance instance)
        {
            _logger.LogStoppingInstance(instance.Id,
                                        instance.Name);

            if (await instance.StopExecutionAsync(_options.Value.TaskStopTimeout))
            {
                _logger.LogInstanceStopped(instance.Id,
                                           instance.Name);
            }
            else
            {
                _logger.LogInstanceNotStoppedWithinTimeOut(instance.Id,
                                                           instance.Name);
            }
        }

        private void UnloadInstanceLoadContext(RuntimeInstance instance)
        {
            _logger.LogUnloadingInstance(instance.PlugInId);

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
                _logger.LogExecutionInstanceCountExceeded(_executingInstances.Count,
                                                          _options.Value.MaxExecutingInstances);

                throw new MaxInstancesRunningException(_executingInstances.Count);
            }
            else
            {
                _logger.LogExecutionInstanceCount(_executingInstances.Count,
                                                  _options.Value.MaxExecutingInstances);
            }
        }
    }
}