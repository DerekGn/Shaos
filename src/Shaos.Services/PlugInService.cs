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
using Shaos.Services.IO;
using Shaos.Services.Repositories;
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
        private readonly IPlugInInstanceRepository _plugInInstanceRepository;
        private readonly IPlugInRepository _plugInRepository;
        private readonly IPlugInTypeValidator _plugInTypeValidator;

        public PlugInService(
            ILogger<PlugInService> logger,
            IInstanceHost instanceHost,
            IFileStoreService fileStoreService,
            IPlugInRepository plugInRepository,
            IPlugInTypeValidator plugInTypeValidator,
            IPlugInInstanceRepository plugInInstanceRepository)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(instanceHost);
            ArgumentNullException.ThrowIfNull(fileStoreService);
            ArgumentNullException.ThrowIfNull(plugInRepository);
            ArgumentNullException.ThrowIfNull(plugInTypeValidator);
            ArgumentNullException.ThrowIfNull(plugInInstanceRepository);

            _logger = logger;
            _instanceHost = instanceHost;
            _fileStoreService = fileStoreService;
            _plugInRepository = plugInRepository;
            _plugInTypeValidator = plugInTypeValidator;
            _plugInInstanceRepository = plugInInstanceRepository;
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInInstanceAsync(
            int id,
            PlugInInstance plugInInstance,
            CancellationToken cancellationToken = default)
        {
            int result = 0;

            await ExecutePlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                _logger.LogInformation("Creating PlugInInstance and adding to runtime. PlugIn: [{Id}]", id);

                result = await _plugInInstanceRepository.CreateAsync(
                    plugIn,
                    plugInInstance,
                    cancellationToken);

                AddInstanceToHost(plugIn, plugInInstance);
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
        public async Task DeletePlugInInstanceAsync(
            int id,
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
        public async Task<PlugInInstance?> SetPlugInInstanceEnableAsync(
            int id,
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

        public async Task StartEnabledInstancesAsync(
            CancellationToken cancellationToken = default)
        {
            var plugIns = _plugInRepository
                .GetAsync(
                    _ => _.Package != null,
                    includeProperties: [nameof(Package), nameof(PlugIn.Instances)],
                    cancellationToken: cancellationToken
                );

            await foreach (var plugIn in plugIns)
            {
                foreach (var instance in plugIn.Instances)
                {
                    if (!_instanceHost.InstanceExists(instance.Id))
                    {
                        AddInstanceToHost(plugIn, instance);

                        if (instance.Enabled)
                        {
                            _logger.LogInformation("Starting Instance [{Id}] Name: [{Name}]",
                                instance.Id,
                                instance.Name);

                            _instanceHost.StartInstance(instance.Id);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task UploadPlugInPackageAsync(
            int id,
            string packageFileName,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(packageFileName);

            await ExecutePlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                if (VerifyPlugState(plugIn, InstanceState.Running, out var id))
                {
                    _logger.LogError("Found running PlugIn Instance Id: [{Id}]", id);

                    throw new PlugInInstanceRunningException(id, "Instance Running");
                }

                _logger.LogInformation("Writing PlugIn Package file [{FileName}]", packageFileName);

                await _fileStoreService.WritePackageFileStreamAsync(
                    plugIn.Id,
                    packageFileName,
                    stream,
                    cancellationToken);

                var plugInFile = _fileStoreService
                    .ExtractPackage(packageFileName, plugIn.Id.ToString())
                    .FirstOrDefault(_ => _.EndsWith(PlugInNamePostFix, StringComparison.OrdinalIgnoreCase));

                if (plugInFile == null)
                {
                    _logger.LogError("No assembly file ending with [{PostFix}] was found in the package [{FileName}] files",
                        PlugInNamePostFix,
                        packageFileName);

                    throw new NoValidPlugInAssemblyFoundException(
                        $"No assembly file ending with [{PlugInNamePostFix}] was found in the package [{packageFileName}] files");
                }

                _plugInTypeValidator.Validate(plugInFile, out var version);

                await CreateOrUpdatePlugInPackageAsync(
                        plugIn,
                        packageFileName,
                        Path.GetFileName(plugInFile),
                        version,
                        cancellationToken);
            },
            false,
            cancellationToken);
        }

        private void AddInstanceToHost(PlugIn plugIn, PlugInInstance instance)
        {
            var assemblyFilePath = Path
                .Combine(_fileStoreService
                .GetAssemblyPath(instance.Id), plugIn.Package!.AssemblyFile);

            _logger.LogInformation("Adding Instance [{Id}] Name: [{Name}] to InstanceHost",
                instance.Id,
                instance.Name);

            _instanceHost.AddInstance(
                instance.Id,
                instance.Name,
                assemblyFilePath);
        }

        private bool CheckPlugInRunning(PlugIn plugIn, out int plugInInstanceId)
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

                var package = new Package()
                {
                    AssemblyFile = assemblyFileName,
                    FileName = packagFileName,
                    Version = version
                };

                await _plugInRepository.CreatePackageAsync(
                    plugIn,
                    package,
                    cancellationToken);
            }
            else
            {
                _logger.LogInformation("Updating a PlugIn package. PlugIn: [{Id}] Assembly: [{Assembly}] Version: [{Version}]",
                    plugIn.Id,
                    assemblyFileName,
                    version);

                plugIn.Package.AssemblyFile = assemblyFileName;
                plugIn.Package.FileName = packagFileName;
                plugIn.Package.Version = version;

                await _plugInRepository.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task ExecutePlugInOperationAsync(
            int id,
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

        private void RemoveInstancesFromHost(PlugIn plugIn)
        {
            foreach (var instance in plugIn.Instances)
            {
                _logger.LogDebug("Removing Instance [{Id}] from instance host", instance.Id);
                _instanceHost.RemoveInstance(instance.Id);
            }
        }

        private bool VerifyPlugState(
            PlugIn plugIn,
            InstanceState state,
            out int runningInstanceId)
        {
            bool result = false;

            runningInstanceId = 0;

            foreach (var instance in plugIn.Instances)
            {
                var executingInstance = _instanceHost.Instances.FirstOrDefault(_ => _.Id == instance.Id);

                if (executingInstance != null && executingInstance.State == state)
                {
                    result = true;
                    runningInstanceId = instance.Id;
                    break;
                }
            }

            return result;
        }
    }
}