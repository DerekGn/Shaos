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
        private readonly IRuntimeInstanceEventHandler _instanceEventHandler;
        private readonly IRuntimeInstanceHost _instanceHost;
        private readonly ILogger<InstanceHostService> _logger;
        private readonly IPlugInConfigurationBuilder _plugInConfigurationBuilder;
        private readonly IPlugInRepository _repository;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Create an instance of a instance host service
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance</param>
        /// <param name="instanceHost">The <see cref="IRuntimeInstanceHost"/> instance</param>
        /// <param name="repository">The <see cref="IPlugInRepository"/> instance</param>
        /// <param name="fileStoreService">The <see cref="IFileStoreService"/> instance</param>
        /// <param name="serviceScopeFactory"></param>
        /// <param name="instanceEventHandler"></param>
        /// <param name="plugInConfigurationBuilder"></param>
        public InstanceHostService(ILogger<InstanceHostService> logger,
                                   IRuntimeInstanceHost instanceHost,
                                   IPlugInRepository repository,
                                   IFileStoreService fileStoreService,
                                   IServiceScopeFactory serviceScopeFactory,
                                   IRuntimeInstanceEventHandler instanceEventHandler,
                                   IPlugInConfigurationBuilder plugInConfigurationBuilder)
        {
            _logger = logger;
            _repository = repository;
            _instanceHost = instanceHost;
            _fileStoreService = fileStoreService;
            _serviceScopeFactory = serviceScopeFactory;
            _instanceEventHandler = instanceEventHandler;
            _plugInConfigurationBuilder = plugInConfigurationBuilder;
        }

        /// <inheritdoc/>
        public async Task<object?> LoadInstanceConfigurationAsync(int id,
                                                                  CancellationToken cancellationToken = default)
        {
            var plugInInstance = await LoadPlugInInstanceAsync(id,
                                                               cancellationToken: cancellationToken);

            var package = plugInInstance!.PlugIn!.Package;

            if (!package!.HasConfiguration && plugInInstance.Configuration!.IsEmptyOrWhiteSpace())
            {
                _logger.LogError("PlugInInstance has no configuration [{Id}]", id);
                throw new PlugInInstanceNotConfiguredException(id);
            }

            var instanceLoadContext = _instanceHost.GetInstanceLoadContext(plugInInstance!.PlugIn!.Id);

            return _plugInConfigurationBuilder.LoadConfiguration(instanceLoadContext.Assembly!,
                                                                 plugInInstance.Configuration);
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

            var plugInInstance = await LoadPlugInInstanceAsync(id,
                                                               cancellationToken: cancellationToken);

            var plugIn = plugInInstance.PlugIn!;
            var package = plugIn.Package!;

            if (plugIn.Package == null)
            {
                _logger.LogWarning("PlugInInstance package not assigned. Id: [{Id}]", id);

                throw new PlugInPackageNotAssignedException(id);
            }

            if (package.HasConfiguration && string.IsNullOrEmpty(plugInInstance.Configuration))
            {
                _logger.LogWarning("PlugIn instance Id: [{Id}] was not found.", id);
                throw new PlugInInstanceNotConfiguredException(id);
            }

            var runtimeInstance = CreatePlugInInstance(plugIn,
                                                       plugInInstance,
                                                       plugInInstance.Configuration);

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
                        RuntimeInstance instance = CreateRuntimeInstance(plugIn,
                                                                         package,
                                                                         plugInInstance,
                                                                         package.HasConfiguration);

                        var runtimeInstance = CreatePlugInInstance(plugIn,
                                                                   plugInInstance,
                                                                   plugInInstance.Configuration);

                        if (!plugInInstance.Enabled)
                        {
                            _logger.LogWarning("{Type}: [{Id}] Name: [{Name}] not enabled for startUp",
                                               nameof(plugInInstance),
                                               plugInInstance.Id,
                                               plugInInstance.Name);
                            continue;
                        }

                        if (package.HasConfiguration && plugInInstance.Configuration!.IsEmptyOrWhiteSpace())
                        {
                            _logger.LogWarning("{Type}: [{Id}] Name: [{Name}] not configured",
                                               nameof(plugInInstance),
                                               plugInInstance.Id,
                                               plugInInstance.Name);

                            continue;
                        }

                        _logger.LogInformation("Starting PlugIn instance. Id: [{Id} Name: [{Name}]]",
                                               instance.Id,
                                               instance.Name);

                        _instanceHost.StartInstance(instance.Id,
                                                    runtimeInstance!);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void StopInstance(int id)
        {
            var instance = _instanceHost.Instances.FirstOrDefault(_ => _.Id == id);

            if (instance != null && instance.ExecutionContext != null)
            {
                _instanceEventHandler.Detach(instance.ExecutionContext.PlugIn);
            }

            _instanceHost.StopInstance(id);
        }

        /// <inheritdoc/>
        public async Task UpdateInstanceConfigurationAsync(int id,
                                                           IEnumerable<KeyValuePair<string, string>> collection,
                                                           CancellationToken cancellationToken = default)
        {
            var plugInInstance = await LoadPlugInInstanceAsync(id, false, cancellationToken);

            var instanceLoadContext = _instanceHost.GetInstanceLoadContext(plugInInstance!.PlugIn!.Id);

            var configuration = _plugInConfigurationBuilder.LoadConfiguration(instanceLoadContext.Assembly!,
                                                                              plugInInstance.Configuration);

            var configurationType = configuration!.GetType();

            foreach (var kvp in collection)
            {
                var value = configurationType.Parse(kvp.Key, kvp.Value);

                configurationType.SetProperty(configuration!, kvp.Key, value);
            }

            var serializedConfiguration = Utf8JsonSerializer.Serialize(configuration);

            plugInInstance!.Configuration = serializedConfiguration;

            await _repository.SaveChangesAsync(cancellationToken);
        }

        private IPlugIn? CreatePlugInInstance(PlugIn plugIn,
                                              PlugInInstance plugInInstance,
                                              string? configuration)
        {
            RuntimeInstanceLoadContext loadContext = _instanceHost.GetInstanceLoadContext(plugIn.Id);

            var scope = _serviceScopeFactory.CreateScope();
            var plugInBuilder = scope.ServiceProvider.GetRequiredService<IPlugInBuilder>();

            plugInBuilder!.Load(loadContext.Assembly!,
                                configuration);

            plugInBuilder.Restore(plugInInstance.Devices.ToSdk());

            var runtimeInstance = plugInBuilder.PlugIn;

            _instanceEventHandler.Attach(runtimeInstance);

            return runtimeInstance;
        }

        private RuntimeInstance CreateRuntimeInstance(PlugIn plugIn,
                                                      Package package,
                                                      PlugInInstance plugInInstance,
                                                      bool configurable)
        {
            var assemblyFilePath = _fileStoreService.GetAssemblyPath(plugIn.Id,
                                                                     package!.AssemblyFile);

            RuntimeInstance instance = _instanceHost.CreateInstance(plugInInstance.Id,
                                                                    plugIn.Id,
                                                                    plugInInstance.Name,
                                                                    assemblyFilePath,
                                                                    configurable);
            return instance;
        }

        private async Task<PlugInInstance> LoadPlugInInstanceAsync(int id,
                                                                   bool withNoTracking = true,
                                                                   CancellationToken cancellationToken = default)
        {
            var plugInInstance = await _repository.GetByIdAsync<PlugInInstance>(id,
                                                                  withNoTracking,
                                                                  includeProperties: [
                                                                      nameof(PlugIn),
                                                                      $"{nameof(PlugIn)}.{nameof(Package)}",
                                                                      $"{nameof(PlugInInstance.Devices)}",
                                                                      $"{nameof(PlugInInstance.Devices)}.{nameof(Device.Parameters)}"],
                                                                  cancellationToken: cancellationToken);

            if (plugInInstance == null)
            {
                _logger.LogWarning("Unable to resolve PlugInInstance. Id: [{Id}]", id);

                throw new PlugInInstanceNotFoundException(id);
            }

            return plugInInstance;
        }
    }
}