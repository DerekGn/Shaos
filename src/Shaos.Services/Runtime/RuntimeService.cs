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
using Shaos.Repository.Models;
using Shaos.Services.IO;

#warning Limit number of executing plugins

namespace Shaos.Services.Runtime
{
    public class RuntimeService : IRuntimeService
    {
        private readonly List<ExecutingInstance> _executingInstances;
        private readonly IFileStoreService _fileStoreService;
        private readonly ILogger<RuntimeService> _logger;

        public RuntimeService(
            ILogger<RuntimeService> logger,
            IFileStoreService fileStoreService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileStoreService = fileStoreService ?? throw new ArgumentNullException(nameof(fileStoreService));

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
        public async Task StartInstanceAsync(
            int id,
            string name,
            string plugInAssemblyPath,
            CancellationToken cancellationToken = default)
        {
            var executingInstance = _executingInstances
                .FirstOrDefault(_ => _.Id == id);

            if (executingInstance == null)
            {
                _logger.LogInformation("Creating ExecutingInstance: [{Id}] Name: [{Name}]",
                    id, name);

                executingInstance = new ExecutingInstance()
                {
                    Id = id,
                    State = ExecutionState.InActive
                };

                _executingInstances.Add(executingInstance);
            }

            if (executingInstance.State == ExecutionState.Active)
            {
                _logger.LogWarning("PlugIn: [{Id}] Name: [{Name}] Already Started", id, name);
            }
            else
            {
                _ = Task
                    .Run(async () => await StartExecutingInstanceAsync(
                        id,
                        name,
                        executingInstance,
                        cancellationToken),
                        cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task StopInstanceAsync(
            int id,
            string name,
            CancellationToken cancellationToken = default)
        {
            var executingInstance = _executingInstances
                .FirstOrDefault(_ => _.Id == id);

            if (executingInstance == null)
            {
                _logger.LogInformation("ExecutingInstance: [{Id}] Name: [{Name}] Not Found",
                    id,
                    name);
            }
            else
            {
                _ = Task
                    .Run(async () => await StopExecutingInstanceAsync(
                        id,
                        name,
                        executingInstance,
                        cancellationToken),
                        cancellationToken);
            }
        }

        private async Task StartExecutingInstanceAsync(
            int id,
            string name,
            ExecutingInstance executingInstance,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting ExecutingInstance PlugInInstance: [{Id}] Name: [{Name}]",
                id,
                name);
        }

        private async Task StopExecutingInstanceAsync(
            int id,
            string name,
            ExecutingInstance executingInstance,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Stopping Executing PlugInInstance: [{Id}] Name: [{Name}]",
                id,
                name);
        }
    }
}