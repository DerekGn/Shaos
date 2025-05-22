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
using Microsoft.Extensions.Primitives;
using Shaos.Repository.Models;
using Shaos.Services.Exceptions;
using Shaos.Services.IO;
using Shaos.Services.Repositories;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Host;
using System.Text.Json;

namespace Shaos.Services
{
    /// <summary>
    ///
    /// </summary>
    public class InstanceHostService : IInstanceHostService
    {
        private readonly IFileStoreService _fileStoreService;
        private readonly IInstanceHost _instanceHost;
        private readonly ILogger<InstanceHostService> _logger;
        private readonly IPlugInInstanceRepository _plugInInstanceRepository;
        private readonly IPlugInRepository _plugInRepository;

        public InstanceHostService(
            ILogger<InstanceHostService> logger,
            IInstanceHost instanceHost,
            IFileStoreService fileStoreService,
            IPlugInRepository plugInRepository,
            IPlugInInstanceRepository plugInInstanceRepository)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(instanceHost);
            ArgumentNullException.ThrowIfNull(fileStoreService);
            ArgumentNullException.ThrowIfNull(plugInRepository);
            ArgumentNullException.ThrowIfNull(plugInInstanceRepository);

            _logger = logger;
            _instanceHost = instanceHost;
            _fileStoreService = fileStoreService;
            _plugInRepository = plugInRepository;
            _plugInInstanceRepository = plugInInstanceRepository;
        }

        /// <inheritdoc/>
        public async Task<object?> LoadInstanceConfigurationAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteInstanceOperationAsync(id, async (instance) =>
            {
                object? instanceConfiguration = null;

                var plugInInstance = await LoadPlugInInstanceAsync(id, cancellationToken) ?? throw new PlugInInstanceNotFoundException(id);

                if (!plugInInstance.PlugIn.Package.HasConfiguration)
                {
                    _logger.LogError("PlugInInstance has no configuration [{Id}]", id);
                    throw new PlugInHasNoConfigurationException(id);
                }

                if (instance.Configuration.IsConfigured)
                {
                    instanceConfiguration = JsonSerializer.Deserialize<object>(instance.Configuration.Configuration!);
                }

                return instanceConfiguration;
            });
        }

        /// <inheritdoc/>
        public async Task StartInstanceAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (_instanceHost.InstanceExists(id))
            {
                var plugInInstance = await LoadPlugInInstanceAsync(id, cancellationToken);

                if (plugInInstance != null)
                {
                    var package = plugInInstance.PlugIn.Package;
                    object? plugInConfiguration = null;

                    if (package != null && package.HasConfiguration)
                    {
                        if (!string.IsNullOrEmpty(plugInInstance.Configuration))
                        {
                            plugInConfiguration = JsonSerializer.Deserialize<object>(plugInInstance.Configuration);
                        }
                        else
                        {
                            throw new PlugInInstanceNotConfiguredException(id);
                        }
                    }

                    _instanceHost.StartInstance(id);
                }
                else
                {
                    _logger.LogWarning("Unable to start a PlugIn instance. PlugIn instance Id: [{Id}] was not found.", id);
                }
            }
            else
            {
                _logger.LogWarning("Unable to start a PlugIn instance. Instance host does not contain instance Id: [{Id}]", id);
            }
        }

        /// <inheritdoc/>
        public async Task StartInstancesAsync(CancellationToken cancellationToken = default)
        {
            var plugIns = _plugInRepository
                .GetAsync(
                    _ => _.Package != null,
                    includeProperties: [nameof(Package), nameof(PlugIn.Instances)],
                    cancellationToken: cancellationToken
                );

            await foreach (var plugIn in plugIns)
            {
                foreach (var plugInInstance in plugIn.Instances)
                {
                    var package = plugIn.Package;

                    var assemblyFile = Path
                        .Combine(_fileStoreService
                        .GetAssemblyPath(plugIn.Id), plugIn.Package!.AssemblyFile);

                    var configuration = new InstanceConfiguration(
                        package!.HasConfiguration,
                        plugInInstance.Configuration);

                    Instance instance = _instanceHost.CreateInstance(
                            plugInInstance.Id,
                            plugIn.Id,
                            plugInInstance.Name,
                            assemblyFile,
                            configuration);

                    if (plugInInstance.Enabled && instance.Configuration.IsConfigured)
                    {
                        _instanceHost.StartInstance(instance.Id);
                    }
                }
            }
        }

        public void StopInstance(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateInstanceConfigurationAsync(int id, IEnumerable<KeyValuePair<string, StringValues>> collection, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///// <inheritdoc/>
        //public async Task StartUpInstancesAsync(CancellationToken cancellationToken = default)
        //{
        //    var plugIns = _plugInRepository
        //        .GetAsync(
        //            _ => _.Package != null,
        //            includeProperties: [nameof(Package), nameof(PlugIn.Instances)],
        //            cancellationToken: cancellationToken
        //        );

        //    await foreach (var plugIn in plugIns)
        //    {
        //        foreach (var instance in plugIn.Instances)
        //        {
        //            var assemblyFile = Path.Combine(_fileStoreService
        //            .GetAssemblyPath(plugIn.Id), plugIn.Package!.AssemblyFile);

        //            _instanceHost
        //                .CreateInstance(instance.Id, plugIn.Id, instance.Name, assemblyFile, plugIn.Package.HasConfiguration);

        //            if (instance.Enabled)
        //            {
        //                object? plugInConfiguration = null;

        //                if (plugIn.Package!.HasConfiguration && string.IsNullOrEmpty(instance.Configuration))
        //                {
        //                    _logger.LogWarning("Instance [{Id}] Name: [{Name}] cannot be started no configuration stored",
        //                        instance.Id,
        //                        instance.Name);
        //                }
        //                else
        //                {
        //                    _logger.LogInformation("Starting Instance [{Id}] Name: [{Name}]",
        //                        instance.Id,
        //                        instance.Name);

        //                    if (!string.IsNullOrEmpty(instance.Configuration))
        //                    {
        //                        plugInConfiguration = JsonSerializer.Deserialize<object>(instance.Configuration);
        //                    }

        //                    _instanceHost.StartInstance(instance.Id, plugInConfiguration);
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <inheritdoc/>
        //public void StopInstance(int id)
        //{
        //    if (_instanceHost.InstanceExists(id))
        //    {
        //        _instanceHost.StopInstance(id);
        //    }
        //    else
        //    {
        //        _logger.LogWarning("Unable to stop a PlugIn instance. PlugIn instance Id: [{Id}] was not found.", id);
        //    }
        //}

        //private async Task<PlugInInstance?> LoadPlugInInstanceAsync(int id, CancellationToken cancellationToken = default)
        //{
        //    return await _plugInInstanceRepository.GetByIdAsync(
        //        id,
        //        includeProperties: [nameof(PlugIn), $"{nameof(PlugIn)}.{nameof(Package)}"],
        //        cancellationToken: cancellationToken);
        //}

        private async Task ExecuteInstanceOperationAsync(
            int id,
            Func<Instance, Task> operation)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

            Instance? instance = _instanceHost.Instances.FirstOrDefault(_ => _.Id == id) ?? throw new InstanceNotFoundException(id);

            await operation(instance);
        }

        private async Task<T> ExecuteInstanceOperationAsync<T>(
            int id,
            Func<Instance, Task<T>> operation)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

            Instance? instance = _instanceHost.Instances.FirstOrDefault(_ => _.Id == id) ?? throw new InstanceNotFoundException(id);

            return await operation(instance);
        }

        private async Task<PlugInInstance?> LoadPlugInInstanceAsync(
           int id,
           CancellationToken cancellationToken = default)
        {
            return await _plugInInstanceRepository.GetByIdAsync(
                id,
                includeProperties: [nameof(PlugIn), $"{nameof(PlugIn)}.{nameof(Package)}"],
                cancellationToken: cancellationToken);
        }
    }
}