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
using NuGet.Packaging;
using NuGet.Versioning;
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

        /// </inheritdoc>
        public async Task StartInstanceAsync(
            PlugInInstance plugInInstance,
            CancellationToken cancellationToken = default)
        {
            var executingPlugIn = _executingInstances
                .FirstOrDefault(_ => _.Id == plugInInstance.Id);

            if (executingPlugIn == null)
            {
                _logger.LogInformation("Creating ExecutingInstance: [{Id}] Name: [{Name}]",
                    plugInInstance.Id, plugInInstance.Name);

                executingPlugIn = new ExecutingInstance()
                {
                    Id = plugInInstance.Id,
                    State = ExecutionState.InActive
                };

                _executingInstances.Add(executingPlugIn);
            }

            if (executingPlugIn.State == ExecutionState.InActive)
            {
                _ = Task
                    .Run(async () => await StartExecutingInstanceAsync(
                        plugInInstance,
                        executingPlugIn,
                        cancellationToken),
                        cancellationToken);
            }
            else
            {
                _logger.LogWarning("PlugIn: [{Id}] Name: [{Name}] Already Started", plugInInstance.Id, plugInInstance.Name);
            }
        }

        /// </inheritdoc>
        public async Task StopInstanceAsync(
            PlugInInstance plugInInstance,
            CancellationToken cancellationToken = default)
        {
            var executingInstance = _executingInstances
                .FirstOrDefault(_ => _.Id == plugInInstance.Id);

            if (executingInstance != null)
            {
                _ = Task
                    .Run(async () => await StopExecutingInstanceAsync(
                        plugInInstance,
                        executingInstance,
                        cancellationToken),
                        cancellationToken);
            }
            else
            {
                _logger.LogInformation("ExecutingInstance: [{Id}] Name: [{Name}] Not Found",
                    plugInInstance.Id,
                    plugInInstance.Name);
            }
        }

        private async Task LoadNuGetPackageDependanciesAsync(int id, NuGetPackage? nuGetPackage, CancellationToken cancellationToken = default)
        {
            using var stream = _fileStoreService.GetNuGetPackageStream(id, nuGetPackage.FileName);

            if(stream != null)
            {
                using PackageArchiveReader reader = new PackageArchiveReader(stream);
                var nuspecReader = await reader.GetNuspecReaderAsync(cancellationToken);

                if (nuspecReader != null)
                {
                }
            }
        }

        private async Task StartExecutingInstanceAsync(
            PlugInInstance plugInInstance,
            ExecutingInstance executingInstance,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting ExecutingInstance PlugInInstance: [{Id}] Name: [{Name}]",
                plugInInstance.Id,
                plugInInstance.Name);

            LoadNuGetPackageDependanciesAsync(plugInInstance.PlugIn.Id, plugInInstance.PlugIn.NuGetPackage);

            //load and
        }

        private async Task StopExecutingInstanceAsync(
            PlugInInstance plugInInstance,
            ExecutingInstance executingInstance,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Stopping Executing PlugInInstance: [{Id}] Name: [{Name}]",
                plugInInstance.Id,
                plugInInstance.Name);
        }
    }
}