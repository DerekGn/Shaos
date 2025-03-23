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
using Shaos.Services.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

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
                _logger.LogWarning("PlugIn: [{Id}] Name: [{Name}] Already Started", plugInInstance.Id, plugInInstance.Name);
            }
            else
            {
                _ = Task.Run(() => StartExecutingInstance(plugIn.Id, plugInInstance.Name, plugIn.Package.AssemblyFile, instance));
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
                    _ = Task
                        .Run(async () => await StopExecutingInstanceAsync(instance));
                }
            }
        }

        private void StartExecutingInstance(
            int id,
            string name,
            string assemblyFileName,
            ExecutingInstance instance)
        {
            instance.SetState(ExecutionState.PlugInLoading);

            var assemblyPath = Path.Combine(_fileStoreService.GetAssemblyPath(id), assemblyFileName);
            var assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(assemblyPath));

            _logger.LogInformation("Loading ExecutingInstance" +
                    "Id: [{Id}] " +
                    "Name: [{Name}] " +
                    "Assembly: [{Assembly}] " +
                    "Path: [{AssemblyPath}] ",
                    instance.Id,
                    name,
                    assemblyName,
                    assemblyPath);

            var assemblyLoadContext = _runtimeAssemblyLoadContextFactory.Create(name, assemblyPath);
            var assembly = assemblyLoadContext.LoadFromAssemblyName(assemblyName);
            var plugIn = _plugInFactory.CreateInstance(assembly, assemblyLoadContext);

            instance.UpdatePlugIn(plugIn);

            _logger.LogInformation("Starting ExecutingInstance " +
                "Id:[{Id}]",
                instance.Id);

            instance.StartPlugInExecution();
        }

        private async Task StopExecutingInstanceAsync(
            ExecutingInstance instance)
        {
            _logger.LogInformation("Stopping Executing PlugInInstance: [{Id}] Name: [{Name}]",
                instance.Id,
                instance.Name);

            await instance.TokenSource!.CancelAsync();

            if (await Task.WhenAny(instance.Task!, Task.Delay(_options.Value.TaskStopTimeout)) == instance.Task)
            {
                _logger.LogInformation("Stopped execution [{Id}] Name: [{Name}]", instance.Id, instance.Name);
            }
            else
            {
                _logger.LogWarning("Instance not stopped within timeout [{Id}] Name: [{Name}]", instance.Id, instance.Name);
            }
        }

        private void VerifyInstanceCount()
        {
            if(_executingInstances.Count == _options.Value.MaxExecutingInstances)
            {
                throw new RuntimeMaxInstancesRunningException();
            }
        }
    }
}