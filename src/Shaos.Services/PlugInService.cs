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
    public partial class PlugInService : IPlugInService
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
        public async Task CreatePlugInAsync(string packageFileName,
                                            string plugInDirectory,
                                            string plugInAssemblyFileName,
                                            CancellationToken cancellationToken = default)
        {
            var plugInTypeInformation = _plugInTypeValidator.Validate(_fileStoreService.GetAssemblyPath(plugInDirectory,
                                                                                                        plugInAssemblyFileName));

            var plugIn = new PlugIn()
            {
                Description = plugInTypeInformation.Description,
                Name = plugInTypeInformation.Name,
            };

            plugIn.PlugInInformation = new PlugInInformation()
            {
                AssemblyFileName = plugInTypeInformation.AssemblyFileName,
                AssemblyVersion = plugInTypeInformation.AssemblyVersion,
                Directory = plugInDirectory,
                HasConfiguration = plugInTypeInformation.HasConfiguration,
                HasLogger = plugInTypeInformation.HasLogger,
                PackageFileName = packageFileName,
                PlugIn = plugIn
            };

            await _repository.CreatePlugInAsync(plugIn,
                                                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInInstanceAsync(int id,
                                                         PlugInInstance plugInInstance,
                                                         CancellationToken cancellationToken = default)
        {
            int result = 0;

            await ExecutePlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                LogPlugInInstanceCreating(id);

                if (plugIn.PlugInInformation != null)
                {
                    var plugInInformation = plugIn.PlugInInformation;

                    result = await _repository.CreatePlugInInstanceAsync(plugIn,
                                                                         plugInInstance,
                                                                         cancellationToken);

                    _instanceHost.CreateInstance(plugInInstance.Id,
                                                 plugIn.Id,
                                                 plugInInstance.Name,
                                                 _fileStoreService.GetAssemblyPath(plugInInformation.Directory, plugInInformation.AssemblyFileName),
                                                 plugInInformation.HasConfiguration);
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

                    await _repository.DeleteAsync<PlugIn>(id,
                                                          cancellationToken);

                    var plugInInformation = plugIn.PlugInInformation!;

                    _fileStoreService.DeletePlugDirectory(plugInInformation.Directory);
                    _fileStoreService.DeletePackage(plugInInformation.PackageFileName);
                }
                else
                {
                    LogPlugInStillRunning(id);

                    throw new PlugInInstanceRunningException(plugInInstanceId,
                                                             $"PlugIn [{id}] still running");
                }
            },
            false,
            cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public void DeletePlugInFiles(string packageFileName,
                                      string plugInDirectory)
        {
            if (!string.IsNullOrWhiteSpace(packageFileName))
            {
                _fileStoreService.DeletePackage(packageFileName);
            }

            if (!string.IsNullOrWhiteSpace(plugInDirectory))
            {
                _fileStoreService.DeletePlugDirectory(plugInDirectory);
            }
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
                    LogInstanceRunning(id);

                    throw new PlugInInstanceRunningException(id, $"Instance [{id}] Running");
                }
                else
                {
                    LogPlugInInstanceDeleting(id);

                    await _repository.DeleteAsync<PlugInInstance>(id,
                                                                  cancellationToken);

                    LogPlugInInstanceDeletingInstanceHost(id);

                    _instanceHost.RemoveInstance(id);
                }
            }
            else
            {
                LogInstanceNotFound(id);
            }
        }

        /// <inheritdoc/>
        public PackageDetails ExtractPackage(string packageFileName)
        {
            _fileStoreService.ExtractPackage(packageFileName,
                                             out var plugInDirectory,
                                             out var files);

            var plugInFile = files?.FirstOrDefault(_ => _.EndsWith(PlugInNamePostFix,
                                                                   StringComparison.OrdinalIgnoreCase))!;

            if (plugInFile == null)
            {
                LogNoAssemblyFound(PlugInNamePostFix, packageFileName);

                throw new NoValidPlugInAssemblyFoundException(
                    $"No assembly file ending with [{PlugInNamePostFix}] was found in the package [{packageFileName}] files");
            }

            return new PackageDetails()
            {
                FileName = packageFileName,
                Files = files,
                PlugInDirectory = plugInDirectory,
                PlugInAssemblyFileName = Path.GetFileName(plugInFile)
            };
        }

        /// <inheritdoc/>
        public PlugInTypeInformation GetPlugInTypeInformation(string plugInDirectory,
                                                              string plugInAssemblyFileName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(plugInDirectory);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(plugInAssemblyFileName);

            return _plugInTypeValidator.Validate(_fileStoreService.GetAssemblyPath(plugInDirectory,
                                                                                   plugInAssemblyFileName));
        }

        /// <inheritdoc/>
        public async Task<object> LoadPlugInInstanceConfigurationAsync(int id,
                                                                       CancellationToken cancellationToken = default)
        {
            var plugInInstance = await _repository.GetByIdAsync<PlugInInstance>(id,
                                                                                includeProperties: [nameof(PlugIn), $"{nameof(PlugIn)}.{nameof(PlugInInformation)}"],
                                                                                cancellationToken: cancellationToken) ?? throw new NotFoundException(id);

            var plugInInformation = (plugInInstance.PlugIn?.PlugInInformation) ?? throw new PlugInPackageNotAssignedException(id);

            if (!plugInInformation.HasConfiguration)
            {
                throw new PlugInPackageHasNoConfigurationException(plugInInstance.PlugIn.Id);
            }

            return _plugInConfigurationBuilder.LoadConfiguration(plugInInformation.Directory,
                                                                 plugInInformation.AssemblyFileName,
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
        public async Task UpdatePlugInPackageAsync(int id,
                                                   string packageFileName,
                                                   string plugInDirectory,
                                                   string plugInAssemblyFileName,
                                                   CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(id, 0);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(packageFileName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(plugInDirectory);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(plugInAssemblyFileName);

            await ExecutePlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                if (VerifyPlugState(plugIn, RuntimeInstanceState.Running, out var ids))
                {
                    LogRunningPlugInFound(ids);

                    throw new PlugInInstancesRunningException(ids,
                                                              "Instances are Running");
                }

                var plugInTypeInformation = _plugInTypeValidator.Validate(_fileStoreService.GetAssemblyPath(plugInDirectory,
                                                                                                            plugInAssemblyFileName));

                if (PlugInPackageChanged(plugIn, plugInTypeInformation))
                {
                    plugIn.Description = plugInTypeInformation.Description;
                    plugIn.Name = plugInTypeInformation.Name;

                    plugIn.PlugInInformation.AssemblyFileName = plugInAssemblyFileName;
                    plugIn.PlugInInformation.AssemblyVersion = plugInTypeInformation.AssemblyVersion.ToString();
                    plugIn.PlugInInformation.Directory = plugInDirectory;
                    plugIn.PlugInInformation.PackageFileName = packageFileName;
                    plugIn.PlugInInformation.HasConfiguration = plugInTypeInformation.HasConfiguration;
                    plugIn.PlugInInformation.HasLogger = plugInTypeInformation.HasLogger;

                    await _repository.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    LogPackageNotChanged(plugIn.Id,
                                         plugIn.Name);
                }
            },
            false,
            cancellationToken);
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
                    LogRunningPlugInFound(ids);

                    throw new PlugInInstancesRunningException(ids, "Instances are Running");
                }

                LogWrittingPlugInPackageFile(packageFileName);

                await _fileStoreService.WritePackageFileStreamAsync(plugIn.Id,
                                                                    packageFileName,
                                                                    stream,
                                                                    cancellationToken);

                var plugInFile = _fileStoreService
                    .ExtractPackage(plugIn.Id, packageFileName)
                    .FirstOrDefault(_ => _.EndsWith(PlugInNamePostFix, StringComparison.OrdinalIgnoreCase));

                if (plugInFile == null)
                {
                    NoAssemblyFilePackageFileFound(PlugInNamePostFix,
                                                   packageFileName);

                    throw new NoValidPlugInAssemblyFoundException(
                        $"No assembly file ending with [{PlugInNamePostFix}] was found in the package [{packageFileName}] files");
                }

                await CreateOrUpdatePlugInInformationAsync(plugIn,
                                                           packageFileName,
                                                           Path.GetFileName(plugInFile),
                                                           _plugInTypeValidator.Validate(plugInFile),
                                                           cancellationToken);
            },
            false,
            cancellationToken);
        }

        private static bool PlugInPackageChanged(PlugIn plugIn,
                                                 PlugInTypeInformation plugInTypeInformation)
        {
            bool result = false;

            result = result && plugIn.Description == plugInTypeInformation.Description;
            result = result && plugIn.Name == plugInTypeInformation.Name;
            result = result && plugIn.PlugInInformation!.AssemblyFileName == plugInTypeInformation.AssemblyFileName;
            result = result && plugIn.PlugInInformation!.AssemblyVersion == plugInTypeInformation.AssemblyVersion;
            result = result && plugIn.PlugInInformation!.Directory == plugInTypeInformation.Directory;
            result = result && plugIn.PlugInInformation!.HasConfiguration == plugInTypeInformation.HasConfiguration;
            result = result && plugIn.PlugInInformation!.HasLogger == plugInTypeInformation.HasLogger;
            result = result && plugIn.PlugInInformation!.TypeName == plugInTypeInformation.TypeName;

            return !result;
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
                        LogFoundRunningInstance(plugInInstanceId);
                        plugInInstanceId = id;
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        private async Task CreateOrUpdatePlugInInformationAsync(PlugIn plugIn,
                                                                string packagFileName,
                                                                string assemblyFileName,
                                                                PlugInTypeInformation plugInTypeInformation,
                                                                CancellationToken cancellationToken)
        {
            if (plugIn.PlugInInformation == null)
            {
                LogCreatingPlugInPackage(plugIn.Id,
                                         assemblyFileName,
                                         plugInTypeInformation.AssemblyVersion);

                var plugInInformation = new PlugInInformation()
                {
                    AssemblyFileName = assemblyFileName,
                    AssemblyVersion = plugInTypeInformation.AssemblyVersion.ToString(),
                    PackageFileName = packagFileName,
                    HasConfiguration = plugInTypeInformation.HasConfiguration,
                    HasLogger = plugInTypeInformation.HasLogger
                };

                await _repository.CreatePlugInInformationAsync(plugIn,
                                                               plugInInformation,
                                                               cancellationToken);
            }
            else
            {
                LogUpdatingPlugInPackage(plugIn.Id,
                                         assemblyFileName,
                                         plugInTypeInformation.AssemblyVersion);

                plugIn.PlugInInformation.AssemblyFileName = assemblyFileName;
                plugIn.PlugInInformation.AssemblyVersion = plugInTypeInformation.AssemblyVersion.ToString();
                plugIn.PlugInInformation.PackageFileName = packagFileName;
                plugIn.PlugInInformation.HasConfiguration = plugInTypeInformation.HasConfiguration;
                plugIn.PlugInInformation.HasLogger = plugInTypeInformation.HasLogger;

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
                                                                [nameof(PlugIn.PlugInInformation)],
                                                                cancellationToken);

            if (plugIn != null)
            {
                await operation(plugIn, cancellationToken);
            }
            else
            {
                LogPlugInNotFound(id);

                throw new NotFoundException(id, $"PlugIn: [{id}] not found");
            }
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Creating a new PlugIn package. PlugIn: [{id}] Assembly: [{assemblyFileName}] Version: [{assemblyVersion}]")]
        private partial void LogCreatingPlugInPackage(int id,
                                                      string assemblyFileName,
                                                      string assemblyVersion);

        [LoggerMessage(Level = LogLevel.Error, Message = "Found running instance [{plugInInstanceId}]")]
        private partial void LogFoundRunningInstance(int plugInInstanceId);

        [LoggerMessage(Level = LogLevel.Warning, Message = "PlugIn Instance Id: [{Id}] not found")]
        private partial void LogInstanceNotFound(int id);
        [LoggerMessage(Level = LogLevel.Warning, Message = "Instance [{id}] Running")]
        private partial void LogInstanceRunning(int id);

        [LoggerMessage(Level = LogLevel.Error, Message = "No assembly file ending with [{plugInNamePostFix}] was found in the package [{packageFileName}] files")]
        private partial void LogNoAssemblyFound(string plugInNamePostFix,
                                                string packageFileName);

        [LoggerMessage(Level = LogLevel.Information, Message = "PlugIn [{id}] Name: [{name}] package not changed")]
        private partial void LogPackageNotChanged(int id,
                                                  string name);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Creating PlugInInstance. PlugIn: [{id}]")]
        private partial void LogPlugInInstanceCreating(int id);

        [LoggerMessage(Level = LogLevel.Information, Message = "Deleting Instance [{id}] from InstanceHost")]
        private partial void LogPlugInInstanceDeleting(int id);

        [LoggerMessage(Level = LogLevel.Information, Message = "Deleting PlugInInstance [{id}]")]
        private partial void LogPlugInInstanceDeletingInstanceHost(int id);

        [LoggerMessage(Level = LogLevel.Warning, Message = "PlugIn: [{id}] not found")]
        private partial void LogPlugInNotFound(int id);

        [LoggerMessage(Level = LogLevel.Warning, Message = "PlugIn [{id}] still running")]
        private partial void LogPlugInStillRunning(int id);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Removing Instance [{id}] from instance host")]
        private partial void LogRemovingInstance(int id);

        [LoggerMessage(Level = LogLevel.Error, Message = "Found running PlugIn Instances Id: [{ids}]")]
        private partial void LogRunningPlugInFound(List<int> ids);

        [LoggerMessage(Level = LogLevel.Information, Message = "Updating a PlugIn package. PlugIn: [{id}] Assembly: [{assemblyFileName}] Version: [{assemblyVersion}]")]
        private partial void LogUpdatingPlugInPackage(int id,
                                                      string assemblyFileName,
                                                      string assemblyVersion);

        [LoggerMessage(Level = LogLevel.Information, Message = "Writing PlugIn Package file [{packageFileName}]")]
        private partial void LogWrittingPlugInPackageFile(string packageFileName);

        [LoggerMessage(Level = LogLevel.Error, Message = "No assembly file ending with [{plugInNamePostFix}] was found in the package [{packageFileName}] files")]
        private partial void NoAssemblyFilePackageFileFound(string plugInNamePostFix,
                                                            string packageFileName);

        private void RemoveInstancesFromHost(PlugIn plugIn)
        {
            foreach (var id in plugIn.Instances.Select(_ => _.Id))
            {
                LogRemovingInstance(id);

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