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
using Shaos.Sdk;
using Shaos.Sdk.Collections.Generic;
using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;
using Shaos.Services.Extensions;

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
    /// The runtime instance event handler
    /// </summary>
    public class RuntimeInstanceEventHandler : IRuntimeInstanceEventHandler
    {
        private readonly ILogger<RuntimeInstanceEventHandler> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Create an instance of a <see cref="IRuntimeInstanceEventHandler"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{T}"/> instance</param>
        /// <param name="serviceScopeFactory"></param>
        public RuntimeInstanceEventHandler(ILogger<RuntimeInstanceEventHandler> logger,
                                           IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <inheritdoc/>
        public void Attach(IPlugIn plugIn)
        {
            ArgumentNullException.ThrowIfNull(plugIn);

            AttachPlugInDevice(plugIn.Devices);

            AttachDevicesListChange(plugIn.Devices);
        }

        /// <inheritdoc/>
        public void Detach(IPlugIn? plugIn)
        {
            ArgumentNullException.ThrowIfNull(plugIn);

            DetachPlugInDevice(plugIn.Devices);

            DetachDevicesListChange(plugIn.Devices);
        }

        internal void AttachDevice(IDevice device)
        {
            _logger.LogDebug("Attaching device signal level and battery level event handler for Device: [{Id}] Name: [{Name}]",
                             device.Id,
                             device.Name);

            device.SignalLevelChanged += DeviceSignalLevelChanged;
            device.BatteryLevelChanged += DeviceBatteryLevelChanged;
        }

        internal void AttachDevicesListChange(IChildObservableList<IPlugIn, IDevice> devices)
        {
            _logger.LogDebug("Attaching device list event handler for PlugIn Id: [{Id}]",
                             devices.Parent.Id);

            devices.ListChanged += DevicesListChangedAsync;
        }

        internal void AttachParametersListChanged(IChildObservableList<IDevice, IBaseParameter> parameters)
        {
            _logger.LogDebug("Attaching parameter list event handler for PlugIn Id: [{Id}] Name: [{Name}]",
                             parameters.Parent.Id,
                             parameters.Parent.Name);

            parameters.ListChanged += ParametersListChangedAsync;
        }

        internal void DetachParametersListChanged(IChildObservableList<IDevice, IBaseParameter> parameters)
        {
            _logger.LogDebug("Detaching parameter list event handler for PlugIn Id: [{Id}] Name: [{Name}]",
                             parameters.Parent.Id,
                             parameters.Parent.Name);

            parameters.ListChanged -= ParametersListChangedAsync;
        }

        internal void DetatchDevice(IDevice device)
        {
            _logger.LogDebug("Detaching device signal level and battery level event handler for Device: [{Id}] Name: [{Name}]",
                             device.Id,
                             device.Name);

            device.SignalLevelChanged -= DeviceSignalLevelChanged;
            device.BatteryLevelChanged -= DeviceBatteryLevelChanged;
        }

        internal async Task ParameterValueChangedAsync(object sender,
                                                       ParameterValueChangedEventArgs<uint> e)
        {
            await SaveParameterChangeAsync<ModelUIntParameter>(sender, (parameter) =>
            {
                _logger.LogTrace("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
                                 parameter.Id,
                                 parameter.Name,
                                 parameter.Value);

                parameter.Value = e.Value;
                parameter.Values.Add(new UIntParameterValue() 
                {
                    Parameter = parameter,
                    ParameterId = parameter.Id,
                    TimeStamp = e.TimeStamp,
                    Value = e.Value
                });
            });
        }

        internal async Task ParameterValueChangedAsync(object sender,
                                                       ParameterValueChangedEventArgs<string> e)
        {
            await SaveParameterChangeAsync<ModelStringParameter>(sender, (parameter) =>
            {
                _logger.LogTrace("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
                                 parameter.Id,
                                 parameter.Name,
                                 parameter.Value);

                parameter.Value = e.Value;
                parameter.Values.Add(new StringParameterValue()
                {
                    Parameter = parameter,
                    ParameterId = parameter.Id,
                    TimeStamp = e.TimeStamp,
                    Value = e.Value
                });
            });
        }

        internal async Task ParameterValueChangedAsync(object sender,
                                                       ParameterValueChangedEventArgs<int> e)
        {
            await SaveParameterChangeAsync<ModelIntParameter>(sender, (parameter) =>
            {
                _logger.LogTrace("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
                                 parameter.Id,
                                 parameter.Name,
                                 parameter.Value);

                parameter.Value = e.Value;
                parameter.Values.Add(new IntParameterValue()
                {
                    Parameter = parameter,
                    ParameterId = parameter.Id,
                    TimeStamp = e.TimeStamp,
                    Value = e.Value
                });
            });
        }

        internal async Task ParameterValueChangedAsync(object sender,
                                                       ParameterValueChangedEventArgs<float> e)
        {
            await SaveParameterChangeAsync<ModelFloatParameter>(sender, (parameter) =>
            {
                _logger.LogTrace("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
                                 parameter.Id,
                                 parameter.Name,
                                 parameter.Value);

                parameter.Value = e.Value;
                parameter.Values.Add(new FloatParameterValue() 
                {
                    Parameter = parameter,
                    ParameterId = parameter.Id,
                    TimeStamp = e.TimeStamp,
                    Value = e.Value
                });
            });
        }

        internal async Task ParameterValueChangedAsync(object sender,
                                                       ParameterValueChangedEventArgs<bool> e)
        {
            await SaveParameterChangeAsync<ModelBoolParameter>(sender, (parameter) =>
            {
                _logger.LogTrace("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
                                 parameter.Id,
                                 parameter.Name,
                                 parameter.Value);

                parameter.Value = e.Value;
                parameter.Values.Add(new BoolParameterValue() 
                {
                    Parameter = parameter,
                    ParameterId = parameter.Id,
                    TimeStamp = e.TimeStamp,
                    Value = e.Value
                });
            });
        }

        private void AttachDeviceAndParameters(IDevice device)
        {
            AttachParametersListChanged(device.Parameters);

            AttachParameters(device.Parameters.ToList());

            AttachDevice(device);
        }

        private void AttachParameter(IBaseParameter parameter)
        {
            _logger.LogDebug("Attaching event handler for parameter Id: [{Id}] Name: [{Name}]",
                             parameter.Id,
                             parameter.Name);

            switch (parameter)
            {
                case IBaseParameter<bool> _:
                    ((IBaseParameter<bool>)parameter).ValueChanged += ParameterValueChangedAsync;
                    break;

                case IBaseParameter<float> _:
                    ((IBaseParameter<float>)parameter).ValueChanged += ParameterValueChangedAsync;
                    break;

                case IBaseParameter<int> _:
                    ((IBaseParameter<int>)parameter).ValueChanged += ParameterValueChangedAsync;
                    break;

                case IBaseParameter<string> _:
                    ((IBaseParameter<string>)parameter).ValueChanged += ParameterValueChangedAsync;
                    break;

                case IBaseParameter<uint> _:
                    ((IBaseParameter<uint>)parameter).ValueChanged += ParameterValueChangedAsync;
                    break;
            }
        }

        private void AttachParameters(List<IBaseParameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                AttachParameter(parameter);
            }
        }

        private void AttachPlugInDevice(IObservableList<IDevice> devices)
        {
            foreach (var device in devices)
            {
                AttachDeviceAndParameters(device);
            }
        }

        private async Task CreateDeviceParametersAsync(IChildObservableList<IDevice, IBaseParameter> deviceParameters,
                                                       IList<IBaseParameter> parameters)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                var modelDevice = await repository.GetByIdAsync<ModelDevice>(deviceParameters.Parent.Id);

                if (modelDevice != null)
                {
                    foreach (var parameter in parameters)
                    {
                        var modelParameter = parameter.ToModel()!;

                        modelParameter.DeviceId = modelDevice.Id;

                        await repository.AddAsync(modelParameter!);

                        await repository.SaveChangesAsync();

                        parameter.SetId(modelParameter.Id);
                    }
                }
                else
                {
                    _logger.LogError("Unable to resolve Device for Id: [{Id}]", deviceParameters.Parent.Id);
                }
            });
        }

        private async Task CreateDevicesAsync(IChildObservableList<IPlugIn, IDevice> plugInDeviceList,
                                              IList<IDevice> devices)
        {
            await ExecuteRepositoryOperationAsync(async (repository) =>
            {
                var plugInInstance = await repository.GetByIdAsync<PlugInInstance>(plugInDeviceList.Parent.Id);

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

                        for (var i = 0; i < modelDevice.Parameters.Count; i++)
                        {
                            device.Parameters[i].SetId(modelDevice.Parameters[i].Id);
                        }
                    }
                }
                else
                {
                    _logger.LogError("Unable to resolve PlugIn for Id: [{Id}]", plugInDeviceList.Parent.Id);
                }
            });
        }

        private async Task DeleteDeviceParametersAsync(IList<IBaseParameter> parameters)
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

        private async Task DeleteDevicesAsync(IList<IDevice> devices)
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

        private void DetachDevicesListChange(IObservableList<IDevice> devices)
        {
            devices.ListChanged -= DevicesListChangedAsync;
        }

        private void DetachParameter(IBaseParameter parameter)
        {
            _logger.LogDebug("Detaching parameter event handler for parameter Id: [{Id}] Name: [{Name}]",
                             parameter.Id,
                             parameter.Name);

            switch (parameter)
            {
                case IBaseParameter<bool> _:
                    ((IBaseParameter<bool>)parameter).ValueChanged -= ParameterValueChangedAsync;
                    break;

                case IBaseParameter<float> _:
                    ((IBaseParameter<float>)parameter).ValueChanged -= ParameterValueChangedAsync;
                    break;

                case IBaseParameter<int> _:
                    ((IBaseParameter<int>)parameter).ValueChanged -= ParameterValueChangedAsync;
                    break;

                case IBaseParameter<string> _:
                    ((IBaseParameter<string>)parameter).ValueChanged -= ParameterValueChangedAsync;
                    break;

                case IBaseParameter<uint> _:
                    ((IBaseParameter<uint>)parameter).ValueChanged -= ParameterValueChangedAsync;
                    break;
            }
        }

        private void DetachParameters(IList<IBaseParameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                DetachParameter(parameter);
            }
        }

        private void DetachPlugInDevice(IObservableList<IDevice> devices)
        {
            foreach (var device in devices)
            {
                DetachParametersListChanged(device.Parameters);

                DetachParameters(device.Parameters.ToList());

                DetatchDevice(device);
            }
        }

        private async Task DeviceBatteryLevelChanged(object sender,
                                                     BatteryLevelChangedEventArgs e)
        {
            if (sender != null)
            {
                await UpdateDeviceAsync(sender as IDevice, (device) =>
                {
                    device.UpdateBatteryLevel(e.BatteryLevel, e.TimeStamp);
                });
            }
        }

        private async Task DeviceSignalLevelChanged(object sender,
                                                    SignalLevelChangedEventArgs e)
        {
            if (sender != null)
            {
                await UpdateDeviceAsync(sender as IDevice, (device) =>
                {
                    device.UpdateSignalLevel(e.SignalLevel, e.TimeStamp);
                });
            }
        }

        private async Task DevicesListChangedAsync(object sender,
                                                   ListChangedEventArgs<IDevice> e)
        {
            if (sender != null)
            {
                switch (e.Action)
                {
                    case ListChangedAction.Add:
                        if (e.Items != null)
                        {
                            await CreateDevicesAsync(sender as IChildObservableList<IPlugIn, IDevice>, e.Items);

                            foreach (var device in e.Items)
                            {
                                AttachDeviceAndParameters(device);
                            }
                        }
                        break;

                    case ListChangedAction.Reset:
                        if (e.Items != null)
                        {
                            await DeleteDevicesAsync(e.Items);
                        }
                        break;

                    case ListChangedAction.Remove:
                        if (e.Items != null)
                        {
                            await DeleteDevicesAsync(e.Items);
                        }
                        break;
                }
            }
        }

        private async Task ExecuteRepositoryOperationAsync(Func<IRepository, Task> operation)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

            await operation(repository);
        }

        private async Task ParametersListChangedAsync(object sender,
                                                      ListChangedEventArgs<IBaseParameter> e)
        {
            if (sender != null)
            {
                switch (e.Action)
                {
                    case ListChangedAction.Add:
                        if (e.Items != null)
                        {
                            await CreateDeviceParametersAsync(sender as IChildObservableList<IDevice, IBaseParameter>,
                                                              e.Items);

                            AttachParameters([.. e.Items]);
                        }
                        break;

                    case ListChangedAction.Reset:
                        if (e.Items != null)
                        {
                            DetachParameters(e.Items);

                            await DeleteDeviceParametersAsync(e.Items);
                        }
                        break;

                    case ListChangedAction.Remove:
                        if (e.Items != null)
                        {
                            DetachParameters(e.Items);

                            await DeleteDeviceParametersAsync(e.Items);
                        }
                        break;
                }
            }
        }

        private async Task SaveParameterChangeAsync<T>(object sender,
                                                       Action<T> operation) where T : BaseEntity
        {
            if (sender != null)
            {
                if (sender is IBaseParameter parameter)
                {
                    await ExecuteRepositoryOperationAsync(async (repository) =>
                    {
                        var modelParameter = await repository.GetByIdAsync<T>(parameter.Id, false);

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
                }
            }
        }

        private async Task UpdateDeviceAsync(IDevice? device,
                                             Action<ModelDevice> updateOperation)
        {
            if (device != null)
            {
                await ExecuteRepositoryOperationAsync(async (repository) =>
                {
                    var modelDevice = await repository.GetByIdAsync<ModelDevice>(device.Id, false);

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
}