using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;
using Shaos.Services.Extensions;
using Shaos.Services.Processing;

using ModelBaseParameter = Shaos.Repository.Models.Devices.Parameters.BaseParameter;
using ModelDevice = Shaos.Repository.Models.Devices.Device;

namespace Shaos.Services.Runtime.Host
{
    public class RuntimeDeviceUpdateHandler : IRuntimeDeviceUpdateHandler
    {
        private readonly ILogger<RuntimeDeviceUpdateHandler> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWorkItemQueue _workItemQueue;

        public RuntimeDeviceUpdateHandler(ILogger<RuntimeDeviceUpdateHandler> logger,
                                          IServiceScopeFactory serviceScopeFactory,
                                          IWorkItemQueue workItemQueue)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _workItemQueue = workItemQueue;
        }

        /// <inheritdoc/>
        public async Task CreateDeviceParametersAsync(int id,
                                                      IList<IBaseParameter> parameters)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                var modelDevice = await repository.GetByIdAsync<ModelDevice>(id);

                if (modelDevice != null)
                {
                    foreach (var parameter in parameters)
                    {
                        var modelParameter = parameter.ToModel()!;

                        modelParameter.DeviceId = modelDevice.Id;

                        await repository.AddAsync(modelParameter!);

                        await repository.SaveChangesAsync();

                        parameter.SetId(modelParameter.Id);

                        _logger.LogDebug("Created Device [{Id}] Name: [{DeviceName}] Parameter: [{ParameterId}] Name: [{ParameterName}]",
                                         id,
                                         modelDevice.Name,
                                         parameter.Id,
                                         parameter.Name);
                    }
                }
                else
                {
                    _logger.LogError("Unable to resolve Device for Id: [{Id}]", id);
                }
            });
        }

        /// <inheritdoc/>
        public async Task CreateDevicesAsync(int id,
                                             IList<IDevice> devices)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                var plugInInstance = await repository.GetByIdAsync<PlugInInstance>(id);

                if (plugInInstance != null)
                {
                    foreach (IDevice device in devices)
                    {
                        var modelDevice = device.ToModel();
                        modelDevice.PlugInInstanceId = plugInInstance.Id;
                        modelDevice.CreateDeviceFeatureParameters();

                        await repository.AddAsync(modelDevice);

                        await repository.SaveChangesAsync();

                        device.SetId(modelDevice.Id);

                        _logger.LogDebug("Created Device [{Id}] Name: [{Name}]",
                                         id,
                                         device.Name);

                        foreach (var parameter in device.Parameters)
                        {
                            var deviceParameter = modelDevice.Parameters.FirstOrDefault(_ => _.Name == parameter.Name && _.ParameterType == parameter.ParameterType && _.Units == parameter.Units);

                            if (deviceParameter != null)
                            {
                                parameter.SetId(deviceParameter.Id);
                            }

                            _logger.LogDebug("Created Device: [{Id}] Name: [{DeviceName}] Parameter: [{ParameterId}] Name: [{ParameterName}]",
                                             id,
                                             device.Name,
                                             parameter.Id,
                                             parameter.Name);
                        }
                    }
                }
                else
                {
                    _logger.LogError("Unable to resolve PlugIn for Id: [{Id}]", id);
                }
            });
        }

        /// <inheritdoc/>
        public async Task DeleteDeviceParametersAsync(IList<IBaseParameter> parameters)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                foreach (var parameter in parameters)
                {
                    _logger.LogInformation("Deleting Parameter Id: [{Id}] Name: [{Name}]",
                                           parameter.Id,
                                           parameter.Name);

                    await repository.DeleteAsync<ModelBaseParameter>(parameter.Id);
                }

                await repository.SaveChangesAsync();
            });
        }

        /// <inheritdoc/>
        public async Task DeleteDevicesAsync(IList<IDevice> devices)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                foreach (var device in devices)
                {
                    _logger.LogInformation("Deleting Device Id: [{Id}] Name: [{Name}]",
                                           device.Id,
                                           device.Name);

                    await repository.DeleteAsync<ModelDevice>(device.Id);
                }

                await repository.SaveChangesAsync();
            });
        }

        /// <inheritdoc/>
        public async Task DeviceBatteryLevelUpdateAsync(IDevice device,
                                                        BatteryLevelChangedEventArgs e)
        {
            await _workItemQueue.QueueAsync(async (cancellationToken) =>
            {
                await UpdateDeviceAsync(device, (device) =>
                {
                    device.UpdateBatteryLevel(e.BatteryLevel,
                                              e.TimeStamp);
                });
            });
        }

        /// <inheritdoc/>
        public async Task DeviceSignalLevelUpdateAsync(IDevice device,
                                                       SignalLevelChangedEventArgs e)
        {
            await _workItemQueue.QueueAsync(async (cancellationToken) =>
            {
                await UpdateDeviceAsync(device, (device) =>
                {
                    device.UpdateSignalLevel(e.SignalLevel,
                                              e.TimeStamp);
                });
            });
        }

        /// <inheritdoc/>
        public async Task SaveParameterChangeAsync<T>(IBaseParameter parameter,
                                                      Action<T> operation) where T : BaseEntity
        {
            await _workItemQueue.QueueAsync(async (cancellationToken) =>
            {
                await ExecuteRepositoryOperationAsync(async (repository) =>
                {
                    var modelParameter = await repository.GetByIdAsync<T>(parameter.Id,
                                                                          false);

                    if (modelParameter != null)
                    {
                        operation(modelParameter);

                        await repository.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogWarning("Unable to resolve [{Type}] With Id: [{Id}]", typeof(T).Name, parameter.Id);
                    }
                });
            });
        }

        private async Task ExecuteRepositoryOperationAsync(Func<IRepository, Task> operation)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

                await operation(repository);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
            }
        }

        private async Task UpdateDeviceAsync(IDevice device,
                                             Action<ModelDevice> updateOperation)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                var modelDevice = await repository.GetByIdAsync<ModelDevice>(device.Id,
                                                                                false,
                                                                                [nameof(ModelDevice.Parameters)]);

                if (modelDevice != null)
                {
                    updateOperation(modelDevice);

                    await repository.SaveChangesAsync();
                }
                else
                {
                    _logger.LogError("Unable to resolve Device for [{Id}]", device.Id);
                }
            });
        }
    }
}