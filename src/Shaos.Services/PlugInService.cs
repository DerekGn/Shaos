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
using Shaos.Sdk;
using Shaos.Services.Exceptions;
using Shaos.Services.Extensions;
using Shaos.Services.IO;
using Shaos.Services.Runtime;
using Shaos.Services.Store;
using System;

namespace Shaos.Services
{
    public class PlugInService : IPlugInService
    {
        private readonly IFileStoreService _fileStoreService;
        private readonly ILogger<PlugInService> _logger;
        private readonly IAssemblyValidationService _plugInValidationService;
        private readonly IRuntimeService _runtimeService;
        private readonly IStore _store;

        public PlugInService(
            ILogger<PlugInService> logger,
            IStore store,
            IRuntimeService runtimeService,
            IFileStoreService fileStoreService,
            IAssemblyValidationService plugInValidationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _runtimeService = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
            _fileStoreService = fileStoreService ?? throw new ArgumentNullException(nameof(fileStoreService));
            _plugInValidationService = plugInValidationService ?? throw new ArgumentNullException(nameof(plugInValidationService));
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInInstanceAsync(
            int id,
            CreatePlugInInstance create,
            CancellationToken cancellationToken = default)
        {
            int result = 0;

            await ExecutePlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                _logger.LogInformation("Creating PlugInInstance. PlugIn: [{Id}]", id);

                result = await _store.CreatePlugInInstanceAsync(
                    create.Name,
                    create.Description,
                    plugIn,
                    cancellationToken);
            },
            true,
            cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task DeletePlugInAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
#warning Check if plugin used and delete plugin archive and files
            _logger.LogInformation("PlugIn [{Id}] Deleting", id);

            // Delete code and compiled assembly files
            //_fileStoreService.DeletePlugInPackageFolder(id);

            await _store.DeleteAsync<PlugIn>(id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeletePlugInInstanceAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("PlugInInstance [{Id}] Deleting", id);

            await _store.DeleteAsync<PlugIn>(id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task SetPlugInInstanceEnableAsync(
            int id,
            bool enable,
            CancellationToken cancellationToken = default)
        {
            await UpdatePlugInInstanceAsync(id, (plugInInstance) =>
            {
                if (plugInInstance != null)
                {
                    plugInInstance.Enabled = enable;
                }
            },
            cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UpdatePlugInInstanceAsync(
            int id,
            UpdatePlugInInstance update,
            CancellationToken cancellationToken = default)
        {
            await UpdatePlugInInstanceAsync(id, (plugInInstance) =>
            {
                if (plugInInstance != null)
                {
                    plugInInstance.Description = update.Description;
                    plugInInstance.Name = update.Name;
                }
            },
            cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<UploadPackageResult> UploadPlugInPackageAsync(
            int id,
            string fileName,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            fileName.ThrowIfNullOrEmpty(nameof(fileName));

            UploadPackageResult result = UploadPackageResult.Success;

            await ExecutePlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                if(VerifyPlugState(plugIn, ExecutionState.Active))
                {
                    _logger.LogInformation("Writing PlugIn Package file [{FileName}]", fileName);
                    result = UploadPackageResult.PlugInRunning;
                }
                else
                {
                    if (!_fileStoreService.PackageExists(fileName))
                    {
                        _logger.LogInformation("Writing PlugIn Package file [{FileName}]", fileName);

                        await _fileStoreService.WritePlugInPackageFileStreamAsync(
                            plugIn.Id,
                            fileName,
                            stream,
                            cancellationToken);

                        var files = _fileStoreService
                            .ExtractPackage(fileName, plugIn.Id.ToString())
                            .Where(_ => Path.GetExtension(_) == ".dll")
                            .Where(_ => !string.Equals(_, "Shaos.Sdk.dll", StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (!ValidPlugInFound(files, out var plugInFile, out var version))
                        {
                            _logger.LogWarning("No valid PlugIn implementation found");
                            result = UploadPackageResult.NoValidPlugIn;
                        }
                        else
                        {
                            await CreateOrUpdatePlugInPackageAsync(plugIn, plugInFile, version, cancellationToken);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("PlugIn Package file already exists [{FileName}]", fileName);

                        result = UploadPackageResult.PackageExists;
                    }
                }
            },
            false,
            cancellationToken);

            return result;
        }

        private async Task CreateOrUpdatePlugInPackageAsync(
            PlugIn plugIn,
            string filePath,
            string version,
            CancellationToken cancellationToken)
        {
            if (plugIn.Package == null)
            {
                _logger.LogInformation("Creating a new PlugIn package. PlugIn: [{Id}] FilePath: [{FilePath}] Version: [{Version}]",
                    plugIn.Id,
                    filePath,
                    version);

                await _store.CreatePlugInPackageAsync(
                    plugIn,
                    filePath,
                    version,
                    cancellationToken);
            }
            else
            {
                _logger.LogInformation("Updating a PlugIn package. PlugIn: [{Id}] FilePath: [{FilePath}] Version: [{Version}]",
                    plugIn.Id,
                    filePath,
                    version);

                await _store.UpdatePlugInPackageAsync(
                    plugIn,
                    filePath,
                    version,
                    cancellationToken);
            }
        }

        private async Task ExecutePlugInOperationAsync(
            int id,
            Func<PlugIn, CancellationToken, Task> operation,
            bool withNoTracking = true,
            CancellationToken cancellationToken = default)
        {
            var plugIn = await _store.GetPlugInByIdAsync(
                id,
                withNoTracking,
                cancellationToken);

            if (plugIn != null)
            {
                await operation(plugIn, cancellationToken);
            }
            else
            {
                _logger.LogWarning("PlugIn: [{Id}] not found", id);
                throw new PlugInNotFoundException($"PlugIn: [{id}] not found");
            }
        }

        private async Task<PlugInInstance?> UpdatePlugInInstanceAsync(
            int id,
            Action<PlugInInstance?> modify,
            CancellationToken cancellationToken = default)
        {
            var plugInInstance = await _store.GetPlugInInstanceByIdAsync(id, cancellationToken);

            modify(plugInInstance);

            await _store.SaveChangesAsync(cancellationToken);

            return plugInInstance;
        }

        private bool ValidPlugInFound(
            IList<string> files,
            out string plugInFile,
            out string version)
        {
            bool validPlugIn = false;
            plugInFile = string.Empty;
            version = string.Empty;

            foreach (var file in files)
            {
                if (_plugInValidationService.ValidateContainsType<IPlugIn>(file, out version))
                {
                    plugInFile = file;
                    validPlugIn = true;
                    break;
                }
            }

            return validPlugIn;
        }

        private bool VerifyPlugState(PlugIn plugIn, ExecutionState state)
        {
            bool result = false;

            foreach (var instance in plugIn.Instances)
            {
                var executingInstance = _runtimeService.GetExecutingInstance(instance.Id);

                if(executingInstance != null && executingInstance.State == state)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
    }
}