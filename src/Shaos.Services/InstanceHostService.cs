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
    public partial class InstanceHostService : IInstanceHostService
    {
        private readonly IFileStoreService _fileStoreService;
        private readonly IRuntimeInstanceEventHandler _instanceEventHandler;
        private readonly IRuntimeInstanceHost _instanceHost;
        private readonly ILogger<InstanceHostService> _logger;
        private readonly IPlugInConfigurationBuilder _plugInConfigurationBuilder;
        private readonly IRepository _repository;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Create an instance of a instance host service
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance</param>
        /// <param name="instanceHost">The <see cref="IRuntimeInstanceHost"/> instance</param>
        /// <param name="repository">The <see cref="IRepository"/> instance</param>
        /// <param name="fileStoreService">The <see cref="IFileStoreService"/> instance</param>
        /// <param name="serviceScopeFactory"></param>
        /// <param name="instanceEventHandler"></param>
        /// <param name="plugInConfigurationBuilder"></param>
        public InstanceHostService(ILogger<InstanceHostService> logger,
                                   IRuntimeInstanceHost instanceHost,
                                   IRepository repository,
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

            var plugInInformation = plugInInstance!.PlugIn!.PlugInInformation;

            if (!plugInInformation!.HasConfiguration && plugInInstance.Configuration!.IsEmptyOrWhiteSpace())
            {
                LogPlugInHasNoConfiguration(id);

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
                LogUnableToStartPlugIn(id);

                throw new InstanceNotFoundException(id);
            }

            var plugInInstance = await LoadPlugInInstanceAsync(id,
                                                               cancellationToken: cancellationToken);

            var plugIn = plugInInstance.PlugIn!;
            var plugInInformation = plugIn.PlugInInformation!;

            if (plugIn.PlugInInformation == null)
            {
                LogPlugInInstancePackageNotAssigned(id);

                throw new PlugInPackageNotAssignedException(id);
            }

            if (plugInInformation.HasConfiguration && string.IsNullOrEmpty(plugInInstance.Configuration))
            {
                LogPlugInInstanceNotConfigured(id);

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
            var plugIns = _repository.GetEnumerableAsync<PlugIn>(_ => _.PlugInInformation != null,
                                                                 includeProperties: [
                                                                     nameof(PlugIn.Instances),
                                                                     nameof(PlugInInformation),
                                                                     $"{nameof(PlugIn.Instances)}.{nameof(PlugInInstance.Devices)}",
                                                                     $"{nameof(PlugIn.Instances)}.{nameof(PlugInInstance.Devices)}.{nameof(Device.Parameters)}"],
                                                                 cancellationToken: cancellationToken);

            await foreach (var plugIn in plugIns)
            {
                var plugInInformation = plugIn.PlugInInformation!;

                if (plugInInformation != null)
                {
                    foreach (var plugInInstance in plugIn.Instances)
                    {
                        RuntimeInstance instance = CreateRuntimeInstance(plugIn,
                                                                         plugInInformation,
                                                                         plugInInstance,
                                                                         plugInInformation.HasConfiguration);

                        var runtimeInstance = CreatePlugInInstance(plugIn,
                                                                   plugInInstance,
                                                                   plugInInstance.Configuration);

                        if (!plugInInstance.Enabled)
                        {
                            LogPlugInInstanceNotEnabled(plugInInstance.Id,
                                                        plugInInstance.Name);
                            continue;
                        }

                        if (plugInInformation.HasConfiguration && plugInInstance.Configuration!.IsEmptyOrWhiteSpace())
                        {
                            LogPlugInInstanceNotConfigured(plugInInstance.Id,
                                                           plugInInstance.Name);

                            continue;
                        }

                        LogStartingPlugInInstance(instance.Id,
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

            using var scope = _serviceScopeFactory.CreateScope();
            var plugInBuilder = scope.ServiceProvider.GetRequiredService<IPlugInBuilder>();

            plugInBuilder!.Load(loadContext.Assembly!,
                                configuration);

            plugInBuilder.Restore(plugInInstance);

            var runtimeInstance = plugInBuilder.PlugIn;

            LogAttachEventHandlers(plugInInstance.Id,
                                   plugInInstance.Name);

            _instanceEventHandler.Attach(runtimeInstance!);

            return runtimeInstance;
        }

        private RuntimeInstance CreateRuntimeInstance(PlugIn plugIn,
                                                      PlugInInformation plugInInformation,
                                                      PlugInInstance plugInInstance,
                                                      bool configurable)
        {
            var plugInAssemblyFilePath = _fileStoreService.GetAssemblyPath(plugInInformation.Directory,
                                                                           plugInInformation!.AssemblyFileName);

            RuntimeInstance instance = _instanceHost.CreateInstance(plugInInstance.Id,
                                                                    plugIn.Id,
                                                                    plugInInstance.Name,
                                                                    plugInAssemblyFilePath,
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
                                                                      $"{nameof(PlugInInstance.Devices)}",
                                                                      $"{nameof(PlugIn)}.{nameof(PlugInInformation)}",
                                                                      $"{nameof(PlugInInstance.Devices)}.{nameof(Device.Parameters)}"],
                                                                  cancellationToken: cancellationToken);

            if (plugInInstance == null)
            {
                LogUnableToResolvePlugInInstance(id);

                throw new PlugInInstanceNotFoundException(id);
            }

            return plugInInstance;
        }

        [LoggerMessage(Level = LogLevel.Warning, Message = "Attaching event handlers to PlugIn Instance Id: [{id}] Name: [{name}]")]
        private partial void LogAttachEventHandlers(int id,
                                                    string name);

        [LoggerMessage(Level = LogLevel.Error, Message = "PlugInInstance has no configuration [{id}]")]
        private partial void LogPlugInHasNoConfiguration(int id);

        [LoggerMessage(Level = LogLevel.Warning, Message = "PlugIn instance Id: [{id}] Name: [{name}] was not configured.")]
        private partial void LogPlugInInstanceNotConfigured(int id,
                                                            string name);

        [LoggerMessage(Level = LogLevel.Warning, Message = "PlugIn instance Id: [{id}] was not found.")]
        private partial void LogPlugInInstanceNotConfigured(int id);

        [LoggerMessage(Level = LogLevel.Warning, Message = "PlugIn Instance Id: [{id}] Name: [{name}] not enabled for startUp")]
        private partial void LogPlugInInstanceNotEnabled(int id,
                                                         string name);

        [LoggerMessage(Level = LogLevel.Warning, Message = "PlugInInstance package not assigned. Id: [{id}]")]
        private partial void LogPlugInInstancePackageNotAssigned(int id);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Starting PlugIn instance. Id: [{id} Name: [{name}]]")]
        private partial void LogStartingPlugInInstance(int id,
                                                       string name);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Unable to resolve PlugIn Instance. Id: [{Id}]")]
        private partial void LogUnableToResolvePlugInInstance(int id);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Unable to start a PlugIn instance Id: [{id}]. Instance host does not contain instance.")]
        private partial void LogUnableToStartPlugIn(int id);
    }
}