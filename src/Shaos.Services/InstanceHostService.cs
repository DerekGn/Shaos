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
using Shaos.Services.Runtime.Host;

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

        public InstanceHostService(ILogger<InstanceHostService> logger,
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
        public async Task<object?> LoadInstanceConfigurationAsync(int id,
                                                                  CancellationToken cancellationToken = default)
        {
            var plugInInstance = await LoadPlugInInstanceAsync(id, cancellationToken) ?? throw new PlugInInstanceNotFoundException(id);

            var package = plugInInstance!.PlugIn!.Package;

            if (!package!.HasConfiguration)
            {
                _logger.LogError("PlugInInstance has no configuration [{Id}]", id);
                throw new PlugInHasNoConfigurationException(id);
            }

            return _instanceHost.LoadConfiguration(id);
        }

        /// <inheritdoc/>
        public async Task StartInstanceAsync(int id,
                                             CancellationToken cancellationToken = default)
        {
            if (_instanceHost.InstanceExists(id))
            {
                _logger.LogInformation("Starting PlugIn instance [{Id}]", id);

                _instanceHost.StartInstance(id);
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

                    var configuration = new InstanceConfiguration(package!.HasConfiguration,
                                                                  plugInInstance.Configuration);

                    Instance instance = _instanceHost.CreateInstance(plugInInstance.Id,
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

        /// <inheritdoc/>
        public void StopInstance(int id)
        {
            _instanceHost.StopInstance(id);
        }

        /// <inheritdoc/>
        public async Task UpdateInstanceConfigurationAsync(int id,
                                                           IEnumerable<KeyValuePair<string, string>> collection,
                                                           CancellationToken cancellationToken = default)
        {
            var configuration = _instanceHost.LoadConfiguration(id);
            var configurationType = configuration.GetType();

            if(configuration == null)
            {
                throw new ConfigurationNotLoadedException(id);
            }

            foreach (var kvp in collection)
            {
                var value = configurationType.Parse(kvp.Key, kvp.Value);

                configurationType.SetProperty(configuration!, kvp.Key, value);
            }

            var plugInInstance = await _plugInInstanceRepository.GetByIdAsync(id,
                                                                              false,
                                                                              cancellationToken: cancellationToken);

            var serializedConfiguration = Utf8JsonSerilizer.Serialize(configuration);


            plugInInstance!.Configuration = serializedConfiguration;

            await _plugInInstanceRepository.SaveChangesAsync(cancellationToken);
        }

        private async Task<PlugInInstance?> LoadPlugInInstanceAsync(int id,
                                                                    CancellationToken cancellationToken = default)
        {
            return await _plugInInstanceRepository.GetByIdAsync(
                id,
                includeProperties: [nameof(PlugIn), $"{nameof(PlugIn)}.{nameof(Package)}"],
                cancellationToken: cancellationToken);
        }
    }
}