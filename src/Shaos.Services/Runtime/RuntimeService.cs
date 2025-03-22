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
using Shaos.Services.Extensions;
using Shaos.Services.IO;
using System.Reflection;

#warning Limit number of executing plugins

namespace Shaos.Services.Runtime
{
    public class RuntimeService : IRuntimeService
    {
        internal readonly List<ExecutingInstance> _executingInstances;
        private readonly IFileStoreService _fileStoreService;
        private readonly ILogger<RuntimeService> _logger;
        private readonly IPlugInFactory _plugInFactory;
        private readonly IRuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;

        public RuntimeService(
            ILogger<RuntimeService> logger,
            IPlugInFactory plugInFactory,
            IFileStoreService fileStoreService,
            IRuntimeAssemblyLoadContextFactory runtimeAssemblyLoadContextFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            int plugInId,
            int plugInInstanceId,
            string name,
            string assemblyFileName)
        {
            name.ThrowIfNullOrEmpty(nameof(name));
            assemblyFileName.ThrowIfNullOrEmpty(nameof(assemblyFileName));

            var instance = _executingInstances
                .FirstOrDefault(_ => _.Id == plugInInstanceId);

            if (instance == null)
            {
                _logger.LogInformation("Creating ExecutingInstance: [{Id}] Name: [{Name}]",
                    plugInInstanceId, name);

                instance = new ExecutingInstance()
                {
                    Id = plugInInstanceId,
                    Name = name,
                };

                _executingInstances.Add(instance);
            }

            if (instance.State == ExecutionState.Active)
            {
                _logger.LogWarning("PlugIn: [{Id}] Name: [{Name}] Already Started", plugInInstanceId, name);
            }
            else
            {
                _ = Task.Run(() => StartExecutingInstance(plugInId, name, assemblyFileName, instance));
            }

            return instance;
        }

        /// <inheritdoc/>
        public void StopInstance(int id)
        {
            var executingInstance = _executingInstances
                .FirstOrDefault(_ => _.Id == id);

            if (executingInstance == null)
            {
                _logger.LogWarning("ExecutingInstance: [{Id}] Not Found", id);
            }
            else
            {
                if(executingInstance.State != ExecutionState.Active)
                {
                    _logger.LogWarning("ExecutingInstance: [{Id}] Name: [{Name}] Not Running",
                        executingInstance.Id,
                        executingInstance.Name);
                }
                else
                {
                    _ = Task
                        .Run(async () => await StopExecutingInstanceAsync(executingInstance));
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
            ExecutingInstance executingInstance)
        {
            _logger.LogInformation("Stopping Executing PlugInInstance: [{Id}] Name: [{Name}]",
                executingInstance.Id,
                executingInstance.Name);

            await executingInstance.TokenSource.CancelAsync();
            executingInstance.Task.Wait();
        }
    }
}