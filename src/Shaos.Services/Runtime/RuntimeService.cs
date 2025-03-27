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
using Shaos.Repository.Models;
using Shaos.Services.Exceptions;
using Shaos.Services.Extensions;
using Shaos.Services.IO;
using System.Reflection;

namespace Shaos.Services.Runtime
{
    public class RuntimeService : IRuntimeService
    {
        internal readonly List<ExecutingInstance> _executingInstances;
        private readonly IFileStoreService _fileStoreService;
        private readonly ILogger<RuntimeService> _logger;
        private readonly IOptions<RuntimeServiceOptions> _options;
        private readonly IPlugInFactory _plugInFactory;
        private readonly IRuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;

        public RuntimeService(
            ILogger<RuntimeService> logger,
            IOptions<RuntimeServiceOptions> options,
            IPlugInFactory plugInFactory,
            IFileStoreService fileStoreService,
            IRuntimeAssemblyLoadContextFactory runtimeAssemblyLoadContextFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _plugInFactory = plugInFactory ?? throw new ArgumentNullException(nameof(plugInFactory));
            _fileStoreService = fileStoreService ?? throw new ArgumentNullException(nameof(fileStoreService));
            _runtimeAssemblyLoadContextFactory = runtimeAssemblyLoadContextFactory ?? throw new ArgumentNullException(nameof(runtimeAssemblyLoadContextFactory));
            _executingInstances = new List<ExecutingInstance>();
        }

        /// </inheritdoc>
        public ExecutingInstance? GetExecutingInstance(int id)
        {
            return _executingInstances.FirstOrDefault(_ => _.Id == id);
        }

        /// <inheritdoc/>
        public IEnumerable<ExecutingInstance> GetExecutingInstances()
        {
            foreach (var executingInstance in _executingInstances)
            {
                yield return executingInstance;
            }
        }

        /// <inheritdoc/>
        public ExecutingInstance StartInstance(
            PlugIn plugIn,
            PlugInInstance plugInInstance)
        {
            ArgumentNullException.ThrowIfNull(nameof(plugIn));
            ArgumentNullException.ThrowIfNull(nameof(plugInInstance));

            ArgumentNullException.ThrowIfNull(plugIn.Package);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(plugIn.Package.AssemblyFile);

            VerifyInstanceCount();

            var instance = _executingInstances
                .FirstOrDefault(_ => _.Id == plugInInstance.Id);

            if (instance == null)
            {
                _logger.LogInformation("Creating ExecutingInstance: [{Id}] Name: [{Name}]",
                    plugInInstance.Id,
                    plugInInstance.Name);

                instance = new ExecutingInstance()
                {
                    Id = plugInInstance.Id,
                    Name = plugInInstance.Name
                };

                _executingInstances.Add(instance);
            }

            if (instance.State == ExecutionState.Active)
            {
                _logger.LogWarning("PlugIn: [{Id}] Name: [{Name}] Already Started", 
                    plugInInstance.Id,
                    plugInInstance.Name);
            }
            else
            {
                _ = Task.Run(() => StartExecutingInstance(plugIn.Id, plugIn.Package.AssemblyFile, instance));
            }

            return instance;
        }

        /// <inheritdoc/>
        public void StopInstance(int id)
        {
            var instance = _executingInstances
                .FirstOrDefault(_ => _.Id == id);

            if (instance == null)
            {
                _logger.LogWarning("Instance: [{Id}] Not Found", id);
            }
            else
            {
                if (instance.State != ExecutionState.Active)
                {
                    _logger.LogWarning("Instance: [{Id}] Name: [{Name}] Not Running",
                        instance.Id,
                        instance.Name);
                }
                else
                {
                    _ = Task.Run(async () => await StopExecutingInstanceAsync(instance));
                }
            }
        }

        private bool LoadInstancePlugInFromAssembly(
            int id,
            string assemblyFileName,
            ExecutingInstance instance)
        {
            try
            {
                instance.State = ExecutionState.PlugInLoading;

                var assemblyPath = Path.Combine(_fileStoreService.GetAssemblyPath(id), assemblyFileName);
                var assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(assemblyPath));

                instance.UnloadingContext = new UnloadingWeakReference<IRuntimeAssemblyLoadContext>(
                    _runtimeAssemblyLoadContextFactory.Create(instance.Name, assemblyPath));
                instance.Assembly = instance.UnloadingContext.Target.LoadFromAssemblyName(assemblyName);
                instance.PlugIn = _plugInFactory.CreateInstance(instance.Assembly, instance.UnloadingContext.Target);

                instance.State = ExecutionState.PlugInLoaded;
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Exception occurred during PlugIn load and create from Assembly. Id: [{Id}] Name: [{Name}]",
                    instance.Id,
                    instance.Name);

                instance.State = ExecutionState.PlugInLoadFailure;
                instance.Exception = exception;
            }

            return instance.State == ExecutionState.PlugInLoaded;
        }

        private void StartExecutingInstance(
            int id,
            string assemblyFileName,
            ExecutingInstance instance)
        {
            _logger.LogInformation("Loading PlugIn from assembly Id: [{Id}] Name: [{Name}] Assembly: [{Assembly}]",
                    instance.Id,
                    instance.Name,
                    assemblyFileName);

            if(LoadInstancePlugInFromAssembly(id, assemblyFileName, instance))
            {
                _logger.LogInformation("Starting PlugIn instance execution Id: [{Id}] Name: [{Name}]",
                instance.Id,
                instance.Name);

                StartInstanceExecution(instance);
            }
        }

        private void StartInstanceExecution(ExecutingInstance instance)
        {
            try
            {
                instance.State = ExecutionState.Activating;
                instance.TokenSource = new CancellationTokenSource();
                instance.Task = Task.Run(
                    async () => await instance.PlugIn!.ExecuteAsync(instance.TokenSource!.Token))
                    .ContinueWith((antecedent) => UpdatePlugInStateOnCompletion(instance, antecedent));
                instance.State = ExecutionState.Active;
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Exception occurred during instance execution start. Id: [{Id}] Name: [{Name}]",
                    instance.Id,
                    instance.Name);

                instance.State = ExecutionState.ActivationFaulted;
                instance.Exception = exception;
            }
        }

        private async Task StopExecutingInstanceAsync(ExecutingInstance instance)
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
            }
            else
            {
                _logger.LogWarning("Instance not stopped within timeout. Id: [{Id}] Name: [{Name}]",
                    instance.Id,
                    instance.Name);
            }
        }

        private void UpdatePlugInStateOnCompletion(ExecutingInstance instance, Task antecedent)
        {
            _logger.LogDebug("Completed PlugIn Task: {NewLine}{Task}",
                Environment.NewLine,
                antecedent.ToLoggingString());

            if ((antecedent.Status == TaskStatus.RanToCompletion) || (antecedent.Status == TaskStatus.Canceled))
            {
                instance.State = ExecutionState.Complete;

                _logger.LogInformation("Instance completed. Id: [{Id}] Name: [{Name}] Task Status: [{Status}]",
                    instance.Id,
                    instance.Name,
                    antecedent.Status);
            }
            else if (antecedent.Status == TaskStatus.Faulted)
            {
                instance.State = ExecutionState.Faulted;
                instance.Exception = antecedent.Exception;

                _logger.LogError(antecedent.Exception, "Instance completed. Id: [{Id}] Name: [{Name}] Task Status: [{Status}]",
                    instance.Id,
                    instance.Name,
                    antecedent.Status);
            }
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