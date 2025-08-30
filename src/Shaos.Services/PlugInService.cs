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
using Shaos.Repository;
using Shaos.Repository.Exceptions;
using Shaos.Repository.Models;
using Shaos.Services.Exceptions;
using Shaos.Services.IO;
using Shaos.Services.Runtime.Host;
using Shaos.Services.Runtime.Validation;
using System.Diagnostics;

namespace Shaos.Services
{
    /// <summary>
    /// The PlugIn service
    /// </summary>
    public class PlugInService : IPlugInService
    {
        private const string PlugInNamePostFix = ".PlugIn.dll";

        private readonly IFileStoreService _fileStoreService;
        private readonly IRuntimeInstanceHost _instanceHost;
        private readonly ILogger<PlugInService> _logger;
        private readonly IPlugInConfigurationBuilder _plugInConfigurationBuilder;
        private readonly IPlugInTypeValidator _plugInTypeValidator;
        private readonly IRepository _repository;

        /// <summary>
        /// Create an instance of a <see cref="PlugInService"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        /// <param name="instanceHost">The <see cref="IRuntimeInstanceHost"/> instance</param>
        /// <param name="repository">The <see cref="IRepository"/> instance</param>
        /// <param name="fileStoreService">The <see cref="IFileStoreService"/> instance</param>
        /// <param name="plugInTypeValidator">The <see cref="IPlugInTypeValidator"/> instance</param>
        /// <param name="plugInConfigurationBuilder"></param>
        public PlugInService(ILogger<PlugInService> logger,
                             IRuntimeInstanceHost instanceHost,
                             IRepository repository,
                             IFileStoreService fileStoreService,
                             IPlugInTypeValidator plugInTypeValidator,
                             IPlugInConfigurationBuilder plugInConfigurationBuilder)
        {
            _logger = logger;
            _instanceHost = instanceHost;
            _repository = repository;
            _fileStoreService = fileStoreService;
            _plugInTypeValidator = plugInTypeValidator;
            _plugInConfigurationBuilder = plugInConfigurationBuilder;
        }

        /// <inheritdoc/>
        public async Task CreatePlugInAsync(string? plugInFile,
                                            string? extractedPath,
                                            CancellationToken cancellationToken = default)
        {
            var packageInformation = GetPackageInformation(extractedPath, plugInFile);

            var plugIn = new PlugIn()
            {
                Description = packageInformation.Description,
                Name = packageInformation.Name,
            };

            plugIn.Package = new Package()
            {
                AssemblyFile = packageInformation.AssemblyFile,
                AssemblyVersion = packageInformation.AssemblyVersion,
                HasConfiguration = packageInformation.HasConfiguration,
                HasLogger = packageInformation.HasLogger
            };

            await _repository.AddAsync(plugIn);

            await _repository.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInInstanceAsync(int id,
                                                         PlugInInstance plugInInstance,
                                                         CancellationToken cancellationToken = default)
        {
            int result = 0;

            await ExecutePlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                _logger.LogInformation("Creating PlugInInstance. PlugIn: [{Id}]", id);

                if (plugIn.Package != null)
                {
                    var package = plugIn.Package;

                    result = await _repository.CreatePlugInInstanceAsync(plugIn,
                                                                         plugInInstance,
                                                                         cancellationToken);

                    _instanceHost.CreateInstance(plugInInstance.Id,
                                                 plugIn.Id,
                                                 plugInInstance.Name,
                                                 _fileStoreService.GetAssemblyPath(plugIn.Id, package.AssemblyFile),
                                                 package.HasConfiguration);
                }
                else
                {
                    throw new PlugInPackageNotAssignedException(id);
                }
            },
            false,
            cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task DeletePlugInAsync(int id,
                                            CancellationToken cancellationToken = default)
        {
            await ExecutePlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                if (!CheckPlugInRunning(plugIn, out var plugInInstanceId))
                {
                    RemoveInstancesFromHost(plugIn);

                    await _repository.DeleteAsync<PlugIn>(id, cancellationToken);

                    await _repository.SaveChangesAsync(cancellationToken);

                    // Delete code and compiled assembly files
                    if (plugIn.Package != null)
                    {
                        _fileStoreService.DeletePackage(id, plugIn.Package.FileName);
                    }
                }
                else
                {
                    _logger.LogWarning("PlugIn [{Id}] still running", id);

                    throw new PlugInInstanceRunningException(plugInInstanceId, $"PlugIn [{id}] still running");
                }
            },
            false,
            cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeletePlugInInstanceAsync(int id,
                                                    CancellationToken cancellationToken = default)
        {
            var instance = _instanceHost.Instances.FirstOrDefault(_ => _.Id == id);

            if (instance != null)
            {
                if (instance.State == RuntimeInstanceState.Running)
                {
                    _logger.LogWarning("Instance [{Id}] Running", id);

                    throw new PlugInInstanceRunningException(id, $"Instance [{id}] Running");
                }
                else
                {
                    _logger.LogInformation("Deleting PlugInInstance [{Id}]", id);

                    await _repository.DeleteAsync<PlugInInstance>(id, cancellationToken);

                    _logger.LogInformation("Deleting Instance [{Id}] from InstanceHost", id);

                    _instanceHost.RemoveInstance(id);
                }
            }
            else
            {
                _logger.LogWarning("Instance [{Id}] not found", id);
            }
        }

        /// <inheritdoc/>
        public void DeletePlugInPackage(string? packagePath,
                                        string? extractedPath)
        {
            if (!string.IsNullOrWhiteSpace(packagePath))
            {
                _fileStoreService.DeletePackage(packagePath);
            }

            if (!string.IsNullOrWhiteSpace(extractedPath))
            {
                _fileStoreService.DeleteExtractedPackage(extractedPath);
            }
        }

        /// <inheritdoc/>
        public PackageDetails ExtractPackage(string packageFileName)
        {
            _fileStoreService.ExtractPackage(packageFileName,
                                            out var extractedPath,
                                            out var files);

            var plugInFile = files?.FirstOrDefault(_ => _.EndsWith(PlugInNamePostFix, StringComparison.OrdinalIgnoreCase))!;

            if (plugInFile == null)
            {
                _logger.LogError("No assembly file ending with [{PostFix}] was found in the package [{FileName}] files",
                                 PlugInNamePostFix,
                                 packageFileName);

                throw new NoValidPlugInAssemblyFoundException(
                    $"No assembly file ending with [{PlugInNamePostFix}] was found in the package [{packageFileName}] files");
            }

            return new PackageDetails()
            {
                FileName = packageFileName,
                ExtractedPath = extractedPath,
                PlugInFile = Path.GetFileName(plugInFile),
                Files = files
            };
        }

        /// <inheritdoc/>
        public PlugInInformation? GetPackageInformation(string extractedPath, string plugInFile)
        {
            return _plugInTypeValidator.Validate(_fileStoreService.GetAssemblyPath(extractedPath, plugInFile));
        }

        /// <inheritdoc/>
        public async Task<object> LoadPlugInInstanceConfigurationAsync(int id,
                                                                       CancellationToken cancellationToken = default)
        {
            var plugInInstance = await _repository.GetByIdAsync<PlugInInstance>(id,
                                                                                includeProperties: [nameof(PlugIn), $"{nameof(PlugIn)}.{nameof(Package)}"],
                                                                                cancellationToken: cancellationToken) ?? throw new NotFoundException(id);

            var package = (plugInInstance.PlugIn?.Package) ?? throw new PlugInPackageNotAssignedException(id);

            if (!package.HasConfiguration)
            {
                throw new PlugInPackageHasNoConfigurationException(plugInInstance.PlugIn.Id);
            }

            return _plugInConfigurationBuilder.LoadConfiguration(plugInInstance.PlugIn.Id,
                                                                 package.AssemblyFile,
                                                                 plugInInstance.Configuration)!;
        }

        /// <inheritdoc/>
        public async Task<PlugInInstance?> SetPlugInInstanceEnableAsync(int id,
                                                                        bool enable,
                                                                        CancellationToken cancellationToken = default)
        {
            var plugInInstance = await _repository.GetByIdAsync<PlugInInstance>(id,
                                                                                false,
                                                                                cancellationToken: cancellationToken) ?? throw new NotFoundException(id);

            plugInInstance.Enabled = enable;

            await _repository.SaveChangesAsync(cancellationToken);

            return plugInInstance;
        }

        /// <inheritdoc/>
        public async Task UploadPlugInPackageAsync(int id,
                                                   string packageFileName,
                                                   Stream stream,
                                                   CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(packageFileName);

            await ExecutePlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                if (VerifyPlugState(plugIn, RuntimeInstanceState.Running, out var ids))
                {
                    _logger.LogError("Found running PlugIn Instances Id: [{Id}]", string.Join(",", ids));

                    throw new PlugInInstancesRunningException(ids, "Instances are Running");
                }

                _logger.LogInformation("Writing PlugIn Package file [{FileName}]", packageFileName);

                await _fileStoreService.WritePackageFileStreamAsync(plugIn.Id,
                                                                    packageFileName,
                                                                    stream,
                                                                    cancellationToken);

                var plugInFile = _fileStoreService
                    .ExtractPackage(plugIn.Id, packageFileName)
                    .FirstOrDefault(_ => _.EndsWith(PlugInNamePostFix, StringComparison.OrdinalIgnoreCase));

                if (plugInFile == null)
                {
                    _logger.LogError("No assembly file ending with [{PostFix}] was found in the package [{FileName}] files",
                        PlugInNamePostFix,
                        packageFileName);

                    throw new NoValidPlugInAssemblyFoundException(
                        $"No assembly file ending with [{PlugInNamePostFix}] was found in the package [{packageFileName}] files");
                }

                await CreateOrUpdatePlugInPackageAsync(plugIn,
                                                       packageFileName,
                                                       Path.GetFileName(plugInFile),
                                                       _plugInTypeValidator.Validate(plugInFile),
                                                       cancellationToken);
            },
            false,
            cancellationToken);
        }

        private bool CheckPlugInRunning(PlugIn plugIn,
                                        out int plugInInstanceId)
        {
            bool result = false;

            plugInInstanceId = 0;

            if (plugIn != null)
            {
                foreach (var id in plugIn.Instances.Select(_ => _.Id))
                {
                    var instance = _instanceHost.Instances.FirstOrDefault(_ => _.Id == id);

                    if (instance != null && instance.State == RuntimeInstanceState.Running)
                    {
                        _logger.LogDebug("Found running instance [{Id}]", plugInInstanceId);
                        plugInInstanceId = id;
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        private async Task CreateOrUpdatePlugInPackageAsync(PlugIn plugIn,
                                                            string packagFileName,
                                                            string assemblyFileName,
                                                            PlugInInformation plugInInformation,
                                                            CancellationToken cancellationToken)
        {
            if (plugIn.Package == null)
            {
                _logger.LogInformation("Creating a new PlugIn package. PlugIn: [{Id}] Assembly: [{Assembly}] Version: [{AssemblyVersion}]",
                    plugIn.Id,
                    assemblyFileName,
                    plugInInformation.AssemblyVersion);

                var package = new Package()
                {
                    AssemblyFile = assemblyFileName,
                    AssemblyVersion = plugInInformation.AssemblyVersion.ToString(),
                    FileName = packagFileName,
                    HasConfiguration = plugInInformation.HasConfiguration,
                    HasLogger = plugInInformation.HasLogger
                };

                await _repository.CreatePackageAsync(plugIn,
                                                     package,
                                                     cancellationToken);
            }
            else
            {
                _logger.LogInformation("Updating a PlugIn package. PlugIn: [{Id}] Assembly: [{Assembly}] Version: [{AssemblyVersion}]",
                    plugIn.Id,
                    assemblyFileName,
                    plugInInformation.AssemblyVersion.ToString());

                plugIn.Package.AssemblyFile = assemblyFileName;
                plugIn.Package.AssemblyVersion = plugInInformation.AssemblyVersion.ToString();
                plugIn.Package.FileName = packagFileName;
                plugIn.Package.HasConfiguration = plugInInformation.HasConfiguration;
                plugIn.Package.HasLogger = plugInInformation.HasLogger;

                await _repository.SaveChangesAsync(cancellationToken);
            }
        }

        [DebuggerStepThrough]
        private async Task ExecutePlugInOperationAsync(int id,
                                                       Func<PlugIn, CancellationToken, Task> operation,
                                                       bool withNoTracking = true,
                                                       CancellationToken cancellationToken = default)
        {
            var plugIn = await _repository.GetByIdAsync<PlugIn>(id,
                                                                withNoTracking,
                                                                cancellationToken: cancellationToken);

            if (plugIn != null)
            {
                await operation(plugIn, cancellationToken);
            }
            else
            {
                _logger.LogWarning("PlugIn: [{Id}] not found", id);
                throw new NotFoundException(id, $"PlugIn: [{id}] not found");
            }
        }

        private void RemoveInstancesFromHost(PlugIn plugIn)
        {
            foreach (var id in plugIn.Instances.Select(_ => _.Id))
            {
                _logger.LogDebug("Removing Instance [{Id}] from instance host", id);
                _instanceHost.RemoveInstance(id);
            }
        }

        private bool VerifyPlugState(PlugIn plugIn,
                                     RuntimeInstanceState state,
                                     out List<int> runningInstanceIds)
        {
            runningInstanceIds = [.. _instanceHost.Instances
                .Where(_ => _.PlugInId == plugIn.Id && _.State == state).
                Select(_=> _.Id)];

            return runningInstanceIds.Count != 0;
        }
    }
}