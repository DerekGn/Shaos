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
using Shaos.Services.Eventing;
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
    public partial class RuntimeDeviceUpdateHandler : IRuntimeDeviceUpdateHandler
    {
        private readonly IDeviceEventQueue _deviceEventQueue;
        private readonly ILogger<RuntimeDeviceUpdateHandler> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWorkItemQueue _workItemQueue;

        /// <summary>
        /// Create an instance of a <see cref="RuntimeDeviceUpdateHandler"/>
        /// </summary>
        /// <param name="logger">A <see cref="ILogger{TCategoryName}"/> instance</param>
        /// <param name="serviceScopeFactory">A <see cref="_serviceScopeFactory"/> instance</param>
        /// <param name="deviceEventQueue">The device event queue</param>
        /// <param name="workItemQueue">The <see cref="IWorkItemQueue"/> instance</param>
        public RuntimeDeviceUpdateHandler(ILogger<RuntimeDeviceUpdateHandler> logger,
                                          IServiceScopeFactory serviceScopeFactory,
                                          IDeviceEventQueue deviceEventQueue,
                                          IWorkItemQueue workItemQueue)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _deviceEventQueue = deviceEventQueue;
            _workItemQueue = workItemQueue;
        }

        /// <inheritdoc/>
        public async Task CreateDeviceParametersAsync(int id,
                                                      IEnumerable<IBaseParameter> parameters)
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

                        LogDeviceCreated(id,
                                         modelDevice.Name,
                                         parameter.Id,
                                         parameter.Name);
                    }

                    await repository.SaveChangesAsync();
                }
                else
                {
                    LogUnableToResolveDevice(id);
                }
            });
        }

        /// <inheritdoc/>
        public async Task CreateDevicesAsync(int id,
                                             IEnumerable<IDevice> devices)
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

                        LogDeviceCreated(id,
                                         device.Name);

                        foreach (var parameter in device.Parameters)
                        {
                            var deviceParameter = modelDevice.Parameters.FirstOrDefault(_ => _.Name == parameter.Name && _.ParameterType == parameter.ParameterType && _.Units == parameter.Units);

                            LogDeviceParameterCreated(id,
                                                      device.Name,
                                                      parameter.Id,
                                                      parameter.Name);
                        }
                    }

                    await repository.SaveChangesAsync();
                }
                else
                {
                    LogUnableToResolvePlugIn(id);
                }
            });
        }

        /// <inheritdoc/>
        public async Task DeleteDeviceParametersAsync(IEnumerable<int> parameterIds)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                foreach (var parameterId in parameterIds)
                {
                    LogParameterDelete(parameterId);

                    await repository.DeleteAsync<ModelBaseParameter>(parameterId);
                }

                await repository.SaveChangesAsync();
            });
        }

        /// <inheritdoc/>
        public async Task DeleteDevicesAsync(IEnumerable<int> deviceIds)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                foreach (var deviceId in deviceIds)
                {
                    LogDeviceDelete(deviceId);

                    await repository.DeleteAsync<ModelDevice>(deviceId);
                }

                await repository.SaveChangesAsync();
            });
        }

        /// <inheritdoc/>
        public async Task DeviceBatteryLevelUpdateAsync(int id,
                                                        uint level,
                                                        DateTime timestamp)
        {
            await _workItemQueue.EnqueueAsync(async (cancellationToken) =>
            {
                await UpdateDeviceBatteryLevelAsync(id,
                                                    level,
                                                    timestamp);

                await PublishDeviceParameterEventAsync(id,
                                                       level,
                                                       timestamp,
                                                       cancellationToken);
            });
        }

        /// <inheritdoc/>
        public async Task DeviceSignalLevelUpdateAsync(int id,
                                                       int level,
                                                       DateTime timestamp)
        {
            await _workItemQueue.EnqueueAsync(async (cancellationToken) =>
            {
                await UpdateDeviceSignalLevelAsync(id, level, timestamp);

                await PublishDeviceParameterEventAsync(id,
                                                      level,
                                                      timestamp,
                                                      cancellationToken);
            });
        }

        /// <inheritdoc/>
        public async Task SaveParameterChangeAsync(int id,
                                                   int value,
                                                   DateTime timestamp)
        {
            await _workItemQueue.EnqueueAsync(async (cancellationToken) =>
            {
                await SaveParameterChangeAsync(id,
                                                  value,
                                                  timestamp,
                                                  cancellationToken);

                await PublishDeviceParameterEventAsync(id,
                                                      value,
                                                      timestamp,
                                                      cancellationToken);
            });
        }

        /// <inheritdoc/>
        public async Task SaveParameterChangeAsync(int id,
                                                   string value,
                                                   DateTime timestamp)
        {
            await _workItemQueue.EnqueueAsync(async (cancellationToken) =>
            {
                await SaveParameterChangeAsync(id,
                                               value,
                                               timestamp,
                                               cancellationToken);

                await PublishDeviceParameterEventAsync(id,
                                                       value,
                                                       timestamp,
                                                       cancellationToken);
            });
        }

        /// <inheritdoc/>
        public async Task SaveParameterChangeAsync(int id,
                                                   float value,
                                                   DateTime timestamp)
        {
            await _workItemQueue.EnqueueAsync(async (cancellationToken) =>
            {
                await SaveParameterChangeAsync(id,
                                               value,
                                               timestamp,
                                               cancellationToken);

                await PublishDeviceParameterEventAsync(id,
                                                      value,
                                                      timestamp,
                                                      cancellationToken);
            });
        }

        /// <inheritdoc/>
        public async Task SaveParameterChangeAsync(int id,
                                                   bool value,
                                                   DateTime timestamp)
        {
            await _workItemQueue.EnqueueAsync(async (cancellationToken) =>
            {
                await SaveParameterChangeAsync(id,
                                               value,
                                               timestamp,
                                               cancellationToken);

                await PublishDeviceParameterEventAsync(id,
                                                      value,
                                                      timestamp,
                                                      cancellationToken);
            });
        }

        /// <inheritdoc/>
        public async Task SaveParameterChangeAsync(int id,
                                                   uint value,
                                                   DateTime timestamp)
        {
            await _workItemQueue.EnqueueAsync(async (cancellationToken) =>
            {
                await SaveParameterChangeAsync(id, value, timestamp, cancellationToken);

                await PublishDeviceParameterEventAsync(id,
                                                      value,
                                                      timestamp,
                                                      cancellationToken);
            });
        }

        internal async Task SaveParameterChangeAsync(int id,
                                                     uint value,
                                                     DateTime timeStamp,
                                                     CancellationToken cancellationToken)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                var parameter = await repository.GetFirstOrDefaultAsync<ModelUIntParameter>(_ => _.ParameterId == id,
                                                                                            false);

                if (parameter != null)
                {
                    LogUpdatingParameter(parameter.Id,
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

                    await repository.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    LogParameterNotFound(id);
                }
            });
        }

        internal async Task SaveParameterChangeAsync(int id,
                                                     bool value,
                                                     DateTime timeStamp,
                                                     CancellationToken cancellationToken)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                var parameter = await repository.GetFirstOrDefaultAsync<ModelBoolParameter>(_ => _.ParameterId == id,
                                                                                            false);

                if (parameter != null)
                {
                    LogUpdatingParameter(parameter.Id,
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

                    await repository.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    LogParameterNotFound(id);
                }
            });
        }

        internal async Task SaveParameterChangeAsync(int id,
                                                     float value,
                                                     DateTime timeStamp,
                                                     CancellationToken cancellationToken)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                var parameter = await repository.GetFirstOrDefaultAsync<ModelFloatParameter>(_ => _.ParameterId == id,
                                                                                             false);

                if (parameter != null)
                {
                    LogUpdatingParameter(parameter.Id,
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

                    await repository.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    LogParameterNotFound(id);
                }
            });
        }

        internal async Task SaveParameterChangeAsync(int id,
                                                     string value,
                                                     DateTime timestamp,
                                                     CancellationToken cancellationToken)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                var parameter = await repository.GetFirstOrDefaultAsync<ModelStringParameter>(_ => _.ParameterId == id,
                                                                                              false);

                if (parameter != null)
                {
                    LogUpdatingParameter(parameter.Id,
                                         parameter.Name,
                                         value);

                    parameter.Value = value;

                    parameter.Values.Add(new StringParameterValue()
                    {
                        Parameter = parameter,
                        ParameterId = parameter.Id,
                        TimeStamp = timestamp,
                        Value = value
                    });

                    await repository.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    LogParameterNotFound(id);
                }
            });
        }

        internal async Task SaveParameterChangeAsync(int id,
                                                     int value,
                                                     DateTime timeStamp,
                                                     CancellationToken cancellationToken)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                var parameter = await repository.GetFirstOrDefaultAsync<ModelIntParameter>(_ => _.ParameterId == id,
                                                                                           false);

                if (parameter != null)
                {
                    LogUpdatingParameter(parameter.Id,
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

                    await repository.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    LogParameterNotFound(id);
                }
            });
        }

        internal async Task UpdateDeviceBatteryLevelAsync(int id,
                                                          uint level,
                                                          DateTime timeStamp)
        {
            await UpdateDeviceAsync(id, (device) =>
            {
                device.UpdateBatteryLevel(level,
                                          timeStamp);
            });
        }

        internal async Task UpdateDeviceSignalLevelAsync(int id,
                                                         int level,
                                                         DateTime timeStamp)
        {
            await UpdateDeviceAsync(id, (device) =>
            {
                device.UpdateSignalLevel(level,
                                         timeStamp);
            });
        }

        private async Task ExecuteRepositoryOperationAsync(Func<IShaosRepository, Task> operation)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IShaosRepository>();

                await operation(repository);
            }
            catch (Exception ex)
            {
                LogUnhandledException(ex);
            }
        }

        [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception occurred")]
        private partial void LogUnhandledException(Exception ex);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Created Device [{id}] Name: [{name}]")]
        private partial void LogDeviceCreated(int id,
                                              string name);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Created Device [{id}] Name: [{deviceName}] Parameter: [{parameterId}] Name: [{parameterName}]")]
        private partial void LogDeviceCreated(int id,
                                              string deviceName,
                                              int parameterId,
                                              string? parameterName);

        [LoggerMessage(Level = LogLevel.Information, Message = "Deleting Device Id: [{id}]")]
        private partial void LogDeviceDelete(int id);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Created Device: [{id}] Name: [{deviceName}] Parameter: [{parameterId}] Name: [{parameterName}]")]
        private partial void LogDeviceParameterCreated(int id,
                                                       string deviceName,
                                                       int parameterId,
                                                       string? parameterName);

        [LoggerMessage(Level = LogLevel.Information, Message = "Deleting Parameter Id: [{id}]")]
        private partial void LogParameterDelete(int id);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Parameter Id: [{Id}] Not Found")]
        private partial void LogParameterNotFound(int id);

        [LoggerMessage(Level = LogLevel.Error, Message = "Unable to resolve Device for Id: [{id}]")]
        private partial void LogUnableToResolveDevice(int id);

        [LoggerMessage(Level = LogLevel.Error, Message = "Unable to resolve PlugIn for Id: [{id}]")]
        private partial void LogUnableToResolvePlugIn(int id);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Updating parameter Id: [{id}] Name: [{name}] Value: [{value}]")]
        private partial void LogUpdatingParameter(int id, string name, string value);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Updating parameter Id: [{id}] Name: [{name}] Value: [{value}]")]
        private partial void LogUpdatingParameter(int id, string name, int value);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Updating parameter Id: [{id}] Name: [{name}] Value: [{value}]")]
        private partial void LogUpdatingParameter(int id, string name, uint value);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Updating parameter Id: [{id}] Name: [{name}] Value: [{value}]")]
        private partial void LogUpdatingParameter(int id, string name, bool value);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Updating parameter Id: [{id}] Name: [{name}] Value: [{value}]")]
        private partial void LogUpdatingParameter(int id, string name, float value);

        private async Task PublishDeviceParameterEventAsync<T>(int id,
                                                               T level,
                                                               DateTime timeStamp,
                                                               CancellationToken cancellationToken)
        {
            await _deviceEventQueue.EnqueueAsync(new DeviceParameterUpdatedEvent<T>()
            {
                Value = level,
                ParameterId = id,
                Timestamp = timeStamp
            }, cancellationToken);
        }

        private async Task UpdateDeviceAsync(int id,
                                             Action<ModelDevice> updateOperation)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                var modelDevice = await repository.GetFirstOrDefaultAsync<ModelDevice>(_ => _.DeviceId == id,
                                                                                       false,
                                                                                       [nameof(ModelDevice.Parameters)]);

                if (modelDevice != null)
                {
                    updateOperation(modelDevice);

                    await repository.SaveChangesAsync();
                }
                else
                {
                    LogUnableToResolveDevice(id);
                }
            });
        }
    }
}