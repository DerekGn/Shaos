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
using Shaos.Services.Extensions;
using Shaos.Services.IO;
using Shaos.Services.Json;
using Shaos.Services.Runtime.Host;

namespace Shaos.Services
{
    public class InstanceHostService : IInstanceHostService
    {
        private readonly IFileStoreService _fileStoreService;
        private readonly IInstanceHost _instanceHost;
        private readonly ILogger<InstanceHostService> _logger;
        private readonly IShaosRepository _repository;

        public InstanceHostService(ILogger<InstanceHostService> logger,
                                   IInstanceHost instanceHost,
                                   IShaosRepository repository,
                                   IFileStoreService fileStoreService)
        {
            ArgumentNullException.ThrowIfNull(instanceHost);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(fileStoreService);

            _logger = logger;
            _repository = repository;
            _instanceHost = instanceHost;
            _fileStoreService = fileStoreService;
        }

        /// <inheritdoc/>
        public async Task<object?> LoadInstanceConfigurationAsync(int id,
                                                                  CancellationToken cancellationToken = default)
        {
            var plugInInstance = await LoadPlugInInstanceAsync(id, cancellationToken) ?? throw new ShaosNotFoundException(id);

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
#warning refactor load of configuration
            if (_instanceHost.InstanceExists(id))
            {
                var plugInInstance = await LoadPlugInInstanceAsync(id, cancellationToken);

                if (plugInInstance != null)
                {
                    object? configuration = null;
                    var package = plugInInstance.PlugIn.Package;

                    if (package != null && package.HasConfiguration)
                    {
                        configuration = _instanceHost.LoadConfiguration(id);

                        if (!string.IsNullOrEmpty(plugInInstance.Configuration))
                        {
                            configuration = Utf8JsonSerilizer.Deserialize(plugInInstance.Configuration,
                                                                          configuration.GetType());
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
            var plugIns = _repository.GetAsync<PlugIn>(_ => _.Package != null,
                                                       includeProperties: [nameof(Package), nameof(PlugIn.Instances)],
                                                       cancellationToken: cancellationToken);

            await foreach (var plugIn in plugIns)
            {
                foreach (var plugInInstance in plugIn.Instances)
                {
                    var package = plugIn.Package;

                    var assemblyFile = _fileStoreService.GetAssemblyPath(plugIn.Id, package!.AssemblyFile);

                    var configuration = new InstanceConfiguration(package!.HasConfiguration,
                                                                  plugInInstance.Configuration);

                    Instance instance = _instanceHost.CreateInstance(plugInInstance.Id,
                                                                     plugIn.Id,
                                                                     plugInInstance.Name,
                                                                     assemblyFile,
                                                                     configuration);

                    if (plugInInstance.Enabled && instance.Configuration.IsConfigured)
                    {
                        _logger.LogInformation("Starting PlugIn instance. Id: [{Id} Name: [{Name}]]",
                            instance.Id,
                            instance.Name);

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

            if (configuration == null)
            {
                throw new ConfigurationNotLoadedException(id);
            }

            foreach (var kvp in collection)
            {
                var value = configurationType.Parse(kvp.Key, kvp.Value);

                configurationType.SetProperty(configuration!, kvp.Key, value);
            }

            var plugInInstance = await _repository.GetByIdAsync<PlugInInstance>(id,
                                                                                false,
                                                                                cancellationToken: cancellationToken);

            var serializedConfiguration = Utf8JsonSerilizer.Serialize(configuration);

            plugInInstance!.Configuration = serializedConfiguration;

            await _repository.SaveChangesAsync(cancellationToken);
        }

        private async Task<PlugInInstance?> LoadPlugInInstanceAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByIdAsync<PlugInInstance>(id,
                                                                  includeProperties: [nameof(PlugIn), $"{nameof(PlugIn)}.{nameof(Package)}"],
                                                                  cancellationToken: cancellationToken);
        }
    }
}