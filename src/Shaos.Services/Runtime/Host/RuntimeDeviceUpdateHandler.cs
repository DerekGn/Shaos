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
using Shaos.Repository.Models.Devices.Parameters;
using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;
using Shaos.Services.Extensions;
using Shaos.Services.Processing;

using ModelBaseParameter = Shaos.Repository.Models.Devices.Parameters.BaseParameter;
using ModelBoolParameter = Shaos.Repository.Models.Devices.Parameters.BoolParameter;
using ModelDevice = Shaos.Repository.Models.Devices.Device;
using ModelFloatParameter = Shaos.Repository.Models.Devices.Parameters.FloatParameter;
using ModelIntParameter = Shaos.Repository.Models.Devices.Parameters.IntParameter;
using ModelStringParameter = Shaos.Repository.Models.Devices.Parameters.StringParameter;
using ModelUIntParameter = Shaos.Repository.Models.Devices.Parameters.UIntParameter;

namespace Shaos.Services.Runtime.Host
{
    /// <summary>
    /// A <see cref="IRuntimeDeviceUpdateHandler"/> that stores updates to a database.
    /// Updates are also published to an event queue.
    /// </summary>
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
                                                        uint level,
                                                        DateTime timeStamp)
        {
            await _workItemQueue.QueueAsync(async (cancellationToken) =>
            {
                await UpdateDeviceAsync(device, (device) =>
                {
                    device.UpdateBatteryLevel(level,
                                              timeStamp);
                });
            });
        }

        /// <inheritdoc/>
        public async Task DeviceSignalLevelUpdateAsync(IDevice device,
                                                       int level,
                                                       DateTime timeStamp)
        {
            await _workItemQueue.QueueAsync(async (cancellationToken) =>
            {
                await UpdateDeviceAsync(device, (device) =>
                {
                    device.UpdateSignalLevel(level,
                                             timeStamp);
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
                        _logger.LogWarning("Unable to resolve [{Type}] With Id: [{Id}]",
                                           typeof(T).Name,
                                           parameter.Id);
                    }
                });
            });
        }

        /// <inheritdoc/>
        public async Task SaveParameterChangeAsync(int id,
                                                   int value,
                                                   DateTime timeStamp)
        {
            await _workItemQueue.QueueAsync(async (cancellationToken) =>
            {
                await ExecuteRepositoryOperationAsync(async (repository) =>
                {
                    var parameter = await repository.GetByIdAsync<ModelIntParameter>(id, false);

                    if (parameter != null)
                    {
                        _logger.LogDebug("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
                                         parameter.Id,
                                         parameter.Name,
                                         value);

                        parameter.Value = value;

                        parameter.Values.Add(new IntParameterValue()
                        {
                            Parameter = parameter,
                            ParameterId = parameter.Id,
                            TimeStamp = timeStamp,
                            Value = value
                        });
                    }
                });
            });
        }

        /// <inheritdoc/>
        public async Task SaveParameterChangeAsync(int id,
                                                   string value,
                                                   DateTime timeStamp)
        {
            await _workItemQueue.QueueAsync(async (cancellationToken) =>
            {
                await ExecuteRepositoryOperationAsync(async (repository) =>
                {
                    var parameter = await repository.GetByIdAsync<ModelStringParameter>(id, false);

                    if (parameter != null)
                    {
                        _logger.LogDebug("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
                                         parameter.Id,
                                         parameter.Name,
                                         value);

                        parameter.Value = value;

                        parameter.Values.Add(new StringParameterValue()
                        {
                            Parameter = parameter,
                            ParameterId = parameter.Id,
                            TimeStamp = timeStamp,
                            Value = value
                        });
                    }
                });
            });
        }

        /// <inheritdoc/>
        public async Task SaveParameterChangeAsync(int id,
                                                   float value,
                                                   DateTime timeStamp)
        {
            await _workItemQueue.QueueAsync(async (cancellationToken) =>
            {
                await ExecuteRepositoryOperationAsync(async (repository) =>
                {
                    var parameter = await repository.GetByIdAsync<ModelFloatParameter>(id, false);

                    if (parameter != null)
                    {
                        _logger.LogDebug("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
                                         parameter.Id,
                                         parameter.Name,
                                         value);

                        parameter.Value = value;

                        parameter.Values.Add(new FloatParameterValue()
                        {
                            Parameter = parameter,
                            ParameterId = parameter.Id,
                            TimeStamp = timeStamp,
                            Value = value
                        });
                    }
                });
            });
        }

        /// <inheritdoc/>
        public async Task SaveParameterChangeAsync(int id,
                                                   bool value,
                                                   DateTime timeStamp)
        {
            await _workItemQueue.QueueAsync(async (cancellationToken) =>
            {
                await ExecuteRepositoryOperationAsync(async (repository) =>
                {
                    var parameter = await repository.GetByIdAsync<ModelBoolParameter>(id, false);

                    if (parameter != null)
                    {
                        _logger.LogDebug("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
                                         parameter.Id,
                                         parameter.Name,
                                         value);

                        parameter.Value = value;

                        parameter.Values.Add(new BoolParameterValue()
                        {
                            Parameter = parameter,
                            ParameterId = parameter.Id,
                            TimeStamp = timeStamp,
                            Value = value
                        });
                    }
                });
            });
        }

        /// <inheritdoc/>
        public async Task SaveParameterChangeAsync(int id,
                                                   uint value,
                                                   DateTime timeStamp)
        {
            await _workItemQueue.QueueAsync(async (cancellationToken) =>
            {
                await ExecuteRepositoryOperationAsync(async (repository) =>
                {
                    var parameter = await repository.GetByIdAsync<ModelUIntParameter>(id, false);

                    if (parameter != null)
                    {
                        _logger.LogDebug("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
                                         parameter.Id,
                                         parameter.Name,
                                         value);

                        parameter.Value = value;

                        parameter.Values.Add(new UIntParameterValue()
                        {
                            Parameter = parameter,
                            ParameterId = parameter.Id,
                            TimeStamp = timeStamp,
                            Value = value
                        });
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
                    _logger.LogError("Unable to resolve Device for [{Id}]",
                                     device.Id);
                }
            });
        }
    }
}