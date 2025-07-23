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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shaos.Repository;
using Shaos.Repository.Exceptions;
using Shaos.Repository.Models;
using Shaos.Repository.Models.Devices;
using Shaos.Sdk;
using Shaos.Services.Exceptions;
using Shaos.Services.Extensions;
using Shaos.Services.IO;
using Shaos.Services.Json;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Host;

namespace Shaos.Services
{
    /// <summary>
    /// An instance hosting service
    /// </summary>
    public class InstanceHostService : IInstanceHostService
    {
        private readonly IFileStoreService _fileStoreService;
        private readonly IInstanceHost _instanceHost;
        private readonly ILogger<InstanceHostService> _logger;
        private readonly IShaosRepository _repository;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Create an instance of a instance host service
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance</param>
        /// <param name="instanceHost">The <see cref="IInstanceHost"/> instance</param>
        /// <param name="repository">The <see cref="IShaosRepository"/> instance</param>
        /// <param name="fileStoreService">The <see cref="IFileStoreService"/> instance</param>
        /// <param name="serviceScopeFactory"></param>
        public InstanceHostService(ILogger<InstanceHostService> logger,
                                   IInstanceHost instanceHost,
                                   IShaosRepository repository,
                                   IFileStoreService fileStoreService,
                                   IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _repository = repository;
            _instanceHost = instanceHost;
            _fileStoreService = fileStoreService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <inheritdoc/>
        public async Task<object?> LoadInstanceConfigurationAsync(int id,
                                                                  CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            //var plugInInstance = await LoadPlugInInstanceAsync(id, cancellationToken) ?? throw new NotFoundException(id);

            //var package = plugInInstance!.PlugIn!.Package;

            //if (!package!.HasConfiguration && plugInInstance.Configuration!.IsEmptyOrWhiteSpace())
            //{
            //    _logger.LogError("PlugInInstance has no configuration [{Id}]", id);
            //    throw new PlugInInstanceNotConfiguredException(id);
            //}

            //return _instanceHost.LoadConfiguration(id);
        }

        /// <inheritdoc/>
        public async Task StartInstanceAsync(int id,
                                             CancellationToken cancellationToken = default)
        {
            if (!_instanceHost.InstanceExists(id))
            {
                _logger.LogWarning("Unable to start a PlugIn instance. Instance host does not contain instance Id: [{Id}]", id);

                throw new InstanceNotFoundException(id);
            }

            var plugInInstance = await LoadPlugInInstanceAsync(id, cancellationToken);

            if (plugInInstance == null)
            {
                _logger.LogWarning("Unable to start a PlugIn instance. PlugIn instance Id: [{Id}] was not found.", id);
                throw new PlugInInstanceNotFoundException(id);
            }

#warning CHECK PLUGIN PACKAGE
            var plugIn = plugInInstance.PlugIn;
            var package = plugIn!.Package!;

            if (package.HasConfiguration && string.IsNullOrEmpty(plugInInstance.Configuration))
            {
                _logger.LogWarning("PlugIn instance Id: [{Id}] was not found.", id);
                throw new PlugInInstanceNotConfiguredException(id);
            }

            var configuration = new InstanceConfiguration(package!.HasConfiguration,
                                                          plugInInstance.Configuration);

            var runtimeInstance = CreatePlugInInstance(plugIn, plugInInstance, configuration);

            _instanceHost.StartInstance(id, runtimeInstance!);
        }

        /// <inheritdoc/>
        public async Task StartInstancesAsync(CancellationToken cancellationToken = default)
        {
            var plugIns = _repository.GetEnumerableAsync<PlugIn>(_ => _.Package != null,
                                                                 includeProperties: [
                                                                     nameof(Package),
                                                                     nameof(PlugIn.Instances),
                                                                     $"{nameof(PlugIn.Instances)}.{nameof(PlugInInstance.Devices)}",
                                                                     $"{nameof(PlugIn.Instances)}.{nameof(PlugInInstance.Devices)}.{nameof(Device.Parameters)}"],
                                                                 cancellationToken: cancellationToken);

            await foreach (var plugIn in plugIns)
            {
                var package = plugIn.Package!;

                if (package != null)
                {
                    foreach (var plugInInstance in plugIn.Instances)
                    {
                        if (package.HasConfiguration && !plugInInstance.Configuration!.IsEmptyOrWhiteSpace())
                        {
                            var configuration = new InstanceConfiguration(package!.HasConfiguration,
                                                                          plugInInstance.Configuration);

                            Instance instance = CreateRuntimeInstance(plugIn, package, plugInInstance);

                            var runtimeInstance = CreatePlugInInstance(plugIn, plugInInstance, configuration);

                            if (plugInInstance.Enabled)
                            {
                                _logger.LogInformation("Starting PlugIn instance. Id: [{Id} Name: [{Name}]]",
                                                   instance.Id,
                                                   instance.Name);

                                _instanceHost.StartInstance(instance.Id, runtimeInstance!);
                            }
                            else
                            {
                                _logger.LogWarning("{Type}: [{Id}] Name: [{Name}] not enabled for startUp",
                                                   nameof(plugInInstance),
                                                   plugInInstance.Id,
                                                   plugInInstance.Name);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("{Type}: [{Id}] Name: [{Name}] not configured",
                                               nameof(plugInInstance),
                                               plugInInstance.Id,
                                               plugInInstance.Name);
                        }
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
#warning TODO refactor
            throw new NotImplementedException();
            //var configuration = _instanceHost.LoadConfiguration(id) ?? throw new ConfigurationNotLoadedException(id);
            //var configurationType = configuration.GetType();

            //foreach (var kvp in collection)
            //{
            //    var value = configurationType.Parse(kvp.Key, kvp.Value);

            //    configurationType.SetProperty(configuration!, kvp.Key, value);
            //}

            //var plugInInstance = await _repository.GetByIdAsync<PlugInInstance>(id,
            //                                                                    false,
            //                                                                    cancellationToken: cancellationToken);

            //var serializedConfiguration = Utf8JsonSerializer.Serialize(configuration);

            //plugInInstance!.Configuration = serializedConfiguration;

            //await _repository.SaveChangesAsync(cancellationToken);
        }

        private IPlugIn? CreatePlugInInstance(PlugIn plugIn, PlugInInstance plugInInstance, InstanceConfiguration configuration)
        {
            InstanceLoadContext loadContext = _instanceHost.GetInstanceLoadContext(plugIn.Id);

            var scope = _serviceScopeFactory.CreateScope();
            var plugInBuilder = scope.ServiceProvider.GetRequiredService<IPlugInBuilder>();

            plugInBuilder!.Load(loadContext.Assembly!,
                                configuration);

            plugInBuilder.Restore(plugInInstance.Devices.ToSdk());

            return plugInBuilder.PlugIn;
        }

        private Instance CreateRuntimeInstance(PlugIn plugIn, Package package, PlugInInstance plugInInstance)
        {
            var assemblyFilePath = _fileStoreService.GetAssemblyPath(plugIn.Id,
                                                                     package!.AssemblyFile);

            Instance instance = _instanceHost.CreateInstance(plugInInstance.Id,
                                                             plugIn.Id,
                                                             plugInInstance.Name,
                                                             assemblyFilePath);
            return instance;
        }

        private async Task<PlugInInstance?> LoadPlugInInstanceAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByIdAsync<PlugInInstance>(id,
                                                                  includeProperties: [
                                                                      nameof(PlugIn),
                                                                      $"{nameof(PlugIn)}.{nameof(Package)}",
                                                                      $"{nameof(PlugInInstance.Devices)}",
                                                                      $"{nameof(PlugInInstance.Devices)}.{nameof(Device.Parameters)}"],
                                                                  cancellationToken: cancellationToken);
        }
    }
}