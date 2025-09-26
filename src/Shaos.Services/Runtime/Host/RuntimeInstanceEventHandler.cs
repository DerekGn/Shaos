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
using Shaos.Repository.Models.Devices.Parameters;
using Shaos.Sdk;
using Shaos.Sdk.Collections.Generic;
using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;
using ModelBoolParameter = Shaos.Repository.Models.Devices.Parameters.BoolParameter;
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
        private readonly IRuntimeDeviceUpdateHandler _runtimeDeviceUpdateHandler;

        /// <summary>
        /// Create an instance of a <see cref="IRuntimeInstanceEventHandler"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{T}"/> instance</param>
        /// <param name="runtimeDeviceUpdateHandler"></param>
        public RuntimeInstanceEventHandler(ILogger<RuntimeInstanceEventHandler> logger,
                                           IRuntimeDeviceUpdateHandler runtimeDeviceUpdateHandler)
        {
            _logger = logger;
            _runtimeDeviceUpdateHandler = runtimeDeviceUpdateHandler;
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
                _logger.LogDebug("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
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
                _logger.LogDebug("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
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
                _logger.LogDebug("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
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
                _logger.LogDebug("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
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
                _logger.LogDebug("Updating parameter Id: [{Id}] Name: [{Name}] Value: [{Value}]",
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

        private static async Task ExecuteDeviceOperationAsync(object sender,
                                                              Func<IDevice, Task> operation)
        {
            if (sender != null)
            {
                if (sender is IDevice device)
                {
                    await operation(device);
                }
            }
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
            await ExecuteDeviceOperationAsync(sender, async (device) =>
            {
                await _runtimeDeviceUpdateHandler.DeviceBatteryLevelUpdateAsync(device, e);
            });
        }

        private async Task DeviceSignalLevelChanged(object sender,
                                                    SignalLevelChangedEventArgs e)
        {
            await ExecuteDeviceOperationAsync(sender, async (device) =>
            {
                await _runtimeDeviceUpdateHandler.DeviceSignalLevelUpdateAsync(device, e);
            });
        }

        private async Task DevicesListChangedAsync(object sender,
                                                   ListChangedEventArgs<IDevice> e)
        {
            if (sender != null)
            {
                if (sender is IChildObservableList<IPlugIn, IDevice> devices)
                {
                    if (e.Items != null)
                    {
                        switch (e.Action)
                        {
                            case ListChangedAction.Add:

                                await _runtimeDeviceUpdateHandler.CreateDevicesAsync(devices.Parent.Id, e.Items);

                                foreach (var device in e.Items)
                                {
                                    AttachDeviceAndParameters(device);
                                }

                                break;

                            case ListChangedAction.Reset:
                                await _runtimeDeviceUpdateHandler.DeleteDevicesAsync(e.Items);

                                break;

                            case ListChangedAction.Remove:
                                await _runtimeDeviceUpdateHandler.DeleteDevicesAsync(e.Items);

                                break;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Event items collection empty");
                    }
                }
                else
                {
                    _logger.LogWarning("Sender is invalid type: [{Type}]", sender.GetType());
                }
            }
        }

        private async Task ParametersListChangedAsync(object sender,
                                                      ListChangedEventArgs<IBaseParameter> e)
        {
            if (sender != null)
            {
                if (sender is IChildObservableList<IDevice, IBaseParameter> deviceParameters)
                {
                    if (e.Items != null)
                    {
                        switch (e.Action)
                        {
                            case ListChangedAction.Add:
                                await _runtimeDeviceUpdateHandler.CreateDeviceParametersAsync(deviceParameters.Parent.Id, e.Items);

                                AttachParameters([.. e.Items]);
                                break;

                            case ListChangedAction.Reset:
                                DetachParameters(e.Items);

                                await _runtimeDeviceUpdateHandler.DeleteDeviceParametersAsync(e.Items);
                                break;

                            case ListChangedAction.Remove:
                                DetachParameters(e.Items);

                                await _runtimeDeviceUpdateHandler.DeleteDeviceParametersAsync(e.Items);
                                break;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Event items collection empty");
                    }
                }
                else
                {
                    _logger.LogWarning("Sender is invalid type: [{Type}]", sender.GetType());
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
                    await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync<T>(parameter, operation);
                }
                else
                {
                    _logger.LogWarning("Sender is invalid type: [{Type}]", sender.GetType());
                }
            }
        }
    }
}