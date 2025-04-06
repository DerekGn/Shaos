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
using Shaos.Services.IO;
using Shaos.Services.Runtime;
using Shaos.Services.Store;
using System.Reflection;

namespace Shaos.Services
{
    public class PlugInService : IPlugInService
    {
        private readonly IFileStoreService _fileStoreService;
        private readonly ILogger<PlugInService> _logger;
        private readonly IRuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;
        private readonly IRuntimeService _runtimeService;
        private readonly IStore _store;

        public PlugInService(
            ILogger<PlugInService> logger,
            IStore store,
            IRuntimeService runtimeService,
            IFileStoreService fileStoreService,
            IRuntimeAssemblyLoadContextFactory runtimeAssemblyLoadContextFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _runtimeService = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
            _fileStoreService = fileStoreService ?? throw new ArgumentNullException(nameof(fileStoreService));
            _runtimeAssemblyLoadContextFactory = runtimeAssemblyLoadContextFactory ?? throw new ArgumentNullException(nameof(runtimeAssemblyLoadContextFactory));
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
            false,
            cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task DeletePlugInAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            await ExecutePlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                if (!CheckPlugInRunning(plugIn, out var plugInInstanceId))
                {
                    // Delete code and compiled assembly files
                    if (plugIn.Package != null)
                    {
                        _fileStoreService.DeletePackage(id, plugIn.Package.FileName);
                    }

                    await _store.DeleteAsync<PlugIn>(id, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("PlugIn [{Id}] still running", id);
                    
                    throw new PlugInInstanceRunningException(plugInInstanceId, $"PlugIn [{id}] still running");
                }
            },
            cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeletePlugInInstanceAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (_runtimeService.GetInstance(id) != null)
            {
                _logger.LogWarning("PlugInInstance [{Id}] Running", id);

                throw new PlugInInstanceRunningException(id, $"PlugInInstance [{id}] Running");
            }
            else
            {
                _logger.LogInformation("PlugInInstance [{Id}] Deleting", id);

                await _store.DeleteAsync<PlugInInstance>(id, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<PlugInInstance?> SetPlugInInstanceEnableAsync(
            int id,
            bool enable,
            CancellationToken cancellationToken = default)
        {
            return await UpdatePlugInInstanceAsync(id, (plugInInstance) =>
            {
                if (plugInInstance != null)
                {
                    plugInInstance.Enabled = enable;
                }
            },
            cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<UploadPackageResult> UploadPlugInPackageAsync(
            int id,
            string packageFileName,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(packageFileName);

            UploadPackageResult result = UploadPackageResult.Success;

            await ExecutePlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                if (VerifyPlugState(plugIn, InstanceState.Active))
                {
                    _logger.LogInformation("Writing PlugIn Package file [{FileName}]", packageFileName);
                    result = UploadPackageResult.PlugInRunning;
                }
                else
                {
                    if (!_fileStoreService.PackageExists(packageFileName))
                    {
                        _logger.LogInformation("Writing PlugIn Package file [{FileName}]", packageFileName);

                        await _fileStoreService.WritePackageFileStreamAsync(
                            plugIn.Id,
                            packageFileName,
                            stream,
                            cancellationToken);

                        var plugInFile = _fileStoreService
                            .ExtractPackage(packageFileName, plugIn.Id.ToString())
                            .FirstOrDefault(_ => _.EndsWith(".PlugIn.dll", StringComparison.OrdinalIgnoreCase));

                        if(plugInFile == null)
                        {
                            _logger.LogWarning("No valid PlugIn implementation found");
                            result = UploadPackageResult.NoValidPlugIn;
                        }
                        else
                        {
                            if(AssemblyContainsPlugIn(plugInFile, out var version))
                            {
                                await CreateOrUpdatePlugInPackageAsync(
                                    plugIn,
                                    packageFileName,
                                    Path.GetFileName(plugInFile),
                                    version,
                                    cancellationToken);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("PlugIn Package file already exists [{FileName}]", packageFileName);

                        result = UploadPackageResult.PackageExists;
                    }
                }
            },
            false,
            cancellationToken);

            return result;
        }

        private bool AssemblyContainsPlugIn(string assemblyFile, out string version)
        {
            var runtimeAssemblyLoadContext = _runtimeAssemblyLoadContextFactory.Create(assemblyFile);
            var unloadingWeakReference = new UnloadingWeakReference<IRuntimeAssemblyLoadContext>(runtimeAssemblyLoadContext);

            bool result = false;

            var plugInAssembly = runtimeAssemblyLoadContext.LoadFromAssemblyPath(assemblyFile);

            version = plugInAssembly.GetName().Version!.ToString();

            result = plugInAssembly.GetTypes().Any(t => typeof(IPlugIn).IsAssignableFrom(t));

            unloadingWeakReference.Dispose();

            return result;
        }

        private bool CheckPlugInRunning(PlugIn plugIn, out int plugInInstanceId)
        {
            bool result = false;

            plugInInstanceId = 0;

            if (plugIn != null)
            {
                foreach (var plugInInstance in plugIn.Instances)
                {
                    if (_runtimeService.GetInstance(plugInInstance.Id) != null)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        private async Task CreateOrUpdatePlugInPackageAsync(
            PlugIn plugIn,
            string packagFileName,
            string assemblyFileName,
            string version,
            CancellationToken cancellationToken)
        {
            if (plugIn.Package == null)
            {
                _logger.LogInformation("Creating a new PlugIn package. PlugIn: [{Id}] Assembly: [{Assembly}] Version: [{Version}]",
                    plugIn.Id,
                    assemblyFileName,
                    version);

                await _store.CreatePlugInPackageAsync(
                    plugIn,
                    packagFileName,
                    assemblyFileName,
                    version,
                    cancellationToken);
            }
            else
            {
                _logger.LogInformation("Updating a PlugIn package. PlugIn: [{Id}] Assembly: [{Assembly}] Version: [{Version}]",
                    plugIn.Id,
                    assemblyFileName,
                    version);

                await _store.UpdatePlugInPackageAsync(
                    plugIn,
                    packagFileName,
                    assemblyFileName,
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
                throw new PlugInNotFoundException(id, $"PlugIn: [{id}] not found");
            }
        }

        private async Task<PlugInInstance?> UpdatePlugInInstanceAsync(
            int id,
            Action<PlugInInstance?> modify,
            CancellationToken cancellationToken = default)
        {
            var plugInInstance = await _store.GetPlugInInstanceByIdAsync(id, cancellationToken);

            if (plugInInstance != null)
            {
                modify(plugInInstance);

                await _store.SaveChangesAsync(cancellationToken);

                return plugInInstance;
            }
            else
            {
                throw new PlugInInstanceNotFoundException(id);
            }
        }
        private bool VerifyPlugState(PlugIn plugIn, InstanceState state)
        {
            bool result = false;

            foreach (var instance in plugIn.Instances)
            {
                var executingInstance = _runtimeService.GetInstance(instance.Id);

                if (executingInstance != null && executingInstance.State == state)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
    }
}