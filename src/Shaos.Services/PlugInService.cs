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
using Shaos.Services.Exceptions;
using Shaos.Services.Extensions;
using Shaos.Services.IO;
using Shaos.Services.Json;
using Shaos.Services.Repositories;
using Shaos.Services.Runtime;
using Shaos.Services.Runtime.Factories;
using Shaos.Services.Runtime.Host;
using Shaos.Services.Runtime.Validation;

namespace Shaos.Services
{
    public class PlugInService : IPlugInService
    {
        private const string PlugInNamePostFix = ".PlugIn.dll";

        private readonly IFileStoreService _fileStoreService;
        private readonly IInstanceHost _instanceHost;
        private readonly ILogger<PlugInService> _logger;
        private readonly IPlugInFactory _plugInFactory;
        private readonly IPlugInInstanceRepository _plugInInstanceRepository;
        private readonly IPlugInRepository _plugInRepository;
        private readonly IPlugInTypeValidator _plugInTypeValidator;
        private readonly IRuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;

        public PlugInService(ILogger<PlugInService> logger,
                             IInstanceHost instanceHost,
                             IPlugInFactory plugInFactory,
                             IFileStoreService fileStoreService,
                             IPlugInRepository plugInRepository,
                             IPlugInTypeValidator plugInTypeValidator,
                             IPlugInInstanceRepository plugInInstanceRepository,
                             IRuntimeAssemblyLoadContextFactory runtimeAssemblyLoadContextFactory)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(instanceHost);
            ArgumentNullException.ThrowIfNull(plugInFactory);
            ArgumentNullException.ThrowIfNull(fileStoreService);
            ArgumentNullException.ThrowIfNull(plugInRepository);
            ArgumentNullException.ThrowIfNull(plugInTypeValidator);
            ArgumentNullException.ThrowIfNull(plugInInstanceRepository);
            ArgumentNullException.ThrowIfNull(runtimeAssemblyLoadContextFactory);

            _logger = logger;
            _instanceHost = instanceHost;
            _plugInFactory = plugInFactory;
            _fileStoreService = fileStoreService;
            _plugInRepository = plugInRepository;
            _plugInTypeValidator = plugInTypeValidator;
            _plugInInstanceRepository = plugInInstanceRepository;
            _runtimeAssemblyLoadContextFactory = runtimeAssemblyLoadContextFactory;
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

                    result = await _plugInInstanceRepository.CreateAsync(plugIn,
                                                                         plugInInstance,
                                                                         cancellationToken);

                    _instanceHost.CreateInstance(plugInInstance.Id,
                                                 plugIn.Id,
                                                 plugInInstance.Name,
                                                 _fileStoreService.GetAssemblyPath(plugIn.Id, package.AssemblyFile),
                                                 new InstanceConfiguration(package!.HasConfiguration));
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

                    // Delete code and compiled assembly files
                    if (plugIn.Package != null)
                    {
                        _fileStoreService.DeletePackage(id, plugIn.Package.FileName);
                    }

                    await _plugInRepository.DeleteAsync(id, cancellationToken);
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
        public async Task DeletePlugInInstanceAsync(int id,
                                                    CancellationToken cancellationToken = default)
        {
            var instance = _instanceHost.Instances.FirstOrDefault(_ => _.Id == id);

            if (instance != null)
            {
                if (instance.State == InstanceState.Running)
                {
                    _logger.LogWarning("Instance [{Id}] Running", id);

                    throw new PlugInInstanceRunningException(id, $"Instance [{id}] Running");
                }
                else
                {
                    _logger.LogInformation("Deleting PlugInInstance [{Id}]", id);

                    await _plugInInstanceRepository.DeleteAsync(id, cancellationToken);

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
        public async Task<object> LoadPlugInInstanceConfigurationAsync(int id,
                                                                       CancellationToken cancellationToken = default)
        {
            var plugInInstance = await _plugInInstanceRepository.GetByIdAsync(
                id,
                includeProperties: [nameof(PlugIn), $"{nameof(PlugIn)}.{nameof(Package)}"],
                cancellationToken: cancellationToken);

            if (plugInInstance == null)
            {
                throw new PlugInInstanceNotFoundException(id);
            }

            object? configuration = null;

            if (string.IsNullOrEmpty(plugInInstance.Configuration))
            {
                configuration = LoadConfiguration(plugInInstance.PlugIn!);
            }
            else
            {
                //configuration = Utf8JsonSerilizer.Deserialize(plugInInstance.Configuration);
            }

            return configuration!;
        }

        /// <inheritdoc/>
        public async Task<PlugInInstance?> SetPlugInInstanceEnableAsync(int id,
                                                                        bool enable,
                                                                        CancellationToken cancellationToken = default)
        {
            var plugInInstance = await _plugInInstanceRepository.GetByIdAsync(id, false, cancellationToken: cancellationToken);

            if (plugInInstance != null)
            {
                plugInInstance.Enabled = enable;

                await _plugInInstanceRepository.SaveChangesAsync(cancellationToken);

                return plugInInstance;
            }
            else
            {
                throw new PlugInInstanceNotFoundException(id);
            }
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
                if (VerifyPlugState(plugIn, InstanceState.Running, out var ids))
                {
                    _logger.LogError("Found running PlugIn Instances Id: [{Id}]", string.Join(",", ids));

                    throw new PlugInInstancesRunningException(ids, "Instances are Running");
                }

                _logger.LogInformation("Writing PlugIn Package file [{FileName}]", packageFileName);

                await _fileStoreService.WritePackageFileStreamAsync(
                    plugIn.Id,
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

                await CreateOrUpdatePlugInPackageAsync(
                        plugIn,
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
                foreach (var plugInInstance in plugIn.Instances)
                {
                    var instance = _instanceHost.Instances.FirstOrDefault(_ => _.Id == plugInInstance.Id);

                    if (instance != null && instance.State == InstanceState.Running)
                    {
                        _logger.LogDebug("Found running instance [{Id}]", plugInInstance.Id);
                        plugInInstanceId = plugInInstance.Id;
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
                                                            PlugInTypeInformation plugInTypeInformation,
                                                            CancellationToken cancellationToken)
        {
            if (plugIn.Package == null)
            {
                _logger.LogInformation("Creating a new PlugIn package. PlugIn: [{Id}] Assembly: [{Assembly}] Version: [{AssemblyVersion}]",
                    plugIn.Id,
                    assemblyFileName,
                    plugInTypeInformation.AssemblyVersion);

                var package = new Package()
                {
                    AssemblyFile = assemblyFileName,
                    AssemblyVersion = plugInTypeInformation.AssemblyVersion.ToString(),
                    FileName = packagFileName,
                    HasConfiguration = plugInTypeInformation.HasConfiguration,
                    HasLogger = plugInTypeInformation.HasLogger
                };

                await _plugInRepository.CreatePackageAsync(
                    plugIn,
                    package,
                    cancellationToken);
            }
            else
            {
                _logger.LogInformation("Updating a PlugIn package. PlugIn: [{Id}] Assembly: [{Assembly}] Version: [{AssemblyVersion}]",
                    plugIn.Id,
                    assemblyFileName,
                    plugInTypeInformation.AssemblyVersion.ToString());

                plugIn.Package.AssemblyFile = assemblyFileName;
                plugIn.Package.AssemblyVersion = plugInTypeInformation.AssemblyVersion.ToString();
                plugIn.Package.FileName = packagFileName;
                plugIn.Package.HasConfiguration = plugInTypeInformation.HasConfiguration;
                plugIn.Package.HasLogger = plugInTypeInformation.HasLogger;

                await _plugInRepository.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task ExecutePlugInOperationAsync(int id,
                                                       Func<PlugIn, CancellationToken, Task> operation,
                                                       bool withNoTracking = true,
                                                       CancellationToken cancellationToken = default)
        {
            var plugIn = await _plugInRepository.GetByIdAsync(
                id,
                withNoTracking,
                cancellationToken: cancellationToken);

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

        private object? LoadConfiguration(PlugIn plugIn)
        {
            var assemblyPath = _fileStoreService.GetAssemblyPath(plugIn.Id,
                                                                 plugIn.Package!.AssemblyFile);

            IRuntimeAssemblyLoadContext? context = null;
            object? configuration = null;

            try
            {
                context = _runtimeAssemblyLoadContextFactory.Create(assemblyPath);
                var assembly = context.LoadFromAssemblyPath(assemblyPath);

                configuration = _plugInFactory.LoadConfiguration(assembly);
            }
            finally
            {
                context?.Unload();
            }

            return configuration!;
        }

        private void RemoveInstancesFromHost(PlugIn plugIn)
        {
            foreach (var instance in plugIn.Instances)
            {
                _logger.LogDebug("Removing Instance [{Id}] from instance host", instance.Id);
                _instanceHost.RemoveInstance(instance.Id);
            }
        }

        private bool VerifyPlugState(PlugIn plugIn,
                                     InstanceState state,
                                     out List<int> runningInstanceIds)
        {
            runningInstanceIds = [.. _instanceHost.Instances
                .Where(_ => _.PlugInId == plugIn.Id && _.State == state).
                Select(_=> _.Id)];

            return runningInstanceIds.Any();
        }
    }
}