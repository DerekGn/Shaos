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
using Shaos.Repository.Models;
using Shaos.Sdk;
using Shaos.Sdk.Collections.Generic;
using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;
using Shaos.Services.Extensions;

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
        private readonly IRepository _repository;

        /// <summary>
        /// Create an instance of a <see cref="IRuntimeInstanceEventHandler"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{T}"/> instance</param>
        /// <param name="repository">The <see cref="IRepository"/></param>
        public RuntimeInstanceEventHandler(ILogger<RuntimeInstanceEventHandler> logger,
                                           IRepository repository)
        {
            _logger = logger;
            _repository = repository;
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

        internal void AttachDevicesListChange(IObservableList<IDevice> devices)
        {
            devices.ListChanged += DevicesListChangedAsync;
        }

        internal void AttachParametersListChanged(IChildObservableList<IBaseParameter, IDevice> parameters)
        {
            parameters.ListChanged += ParametersListChangedAsync;
        }

        private void AttachDevice(IDevice device)
        {
            device.DeviceChanged += DeviceChangedAsync;
        }

        private void AttachParameter(IBaseParameter parameter)
        {
            var type = parameter.GetType();

            switch (type)
            {
                case Type _ when type == typeof(BoolParameter):
                    ((BoolParameter)parameter).ValueChanged += ParameterValueChangedAsync;
                    break;

                case Type _ when type == typeof(FloatParameter):
                    ((FloatParameter)parameter).ValueChanged += ParameterValueChangedAsync;
                    break;

                case Type _ when type == typeof(IntParameter):
                    ((IntParameter)parameter).ValueChanged += ParameterValueChangedAsync;
                    break;

                case Type _ when type == typeof(StringParameter):
                    ((StringParameter)parameter).ValueChanged += ParameterValueChangedAsync;
                    break;

                case Type _ when type == typeof(UIntParameter):
                    ((UIntParameter)parameter).ValueChanged += ParameterValueChangedAsync;
                    break;
            }
        }

        private void AttachParameters(IChildObservableList<IBaseParameter, IDevice> parameters)
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
                AttachParametersListChanged(device.Parameters);

                AttachParameters(device.Parameters);

                AttachDevice(device);
            }
        }

        private async Task CreateDeviceParametersAsync(IList<IBaseParameter> items)
        {
            foreach (var item in items)
            {
                var modelParameter = item.ToModel();

                await _repository.AddAsync(modelParameter!);
            }

            await _repository.SaveChangesAsync();
        }

        private async Task CreateDevicesAsync(IList<IDevice> devices)
        {
            foreach (IDevice device in devices)
            {
                var modelDevice = device.ToModel();

                await _repository.AddAsync(modelDevice);

                device.SetId(modelDevice.Id);
            }

            await _repository.SaveChangesAsync();
        }

        private async Task DeleteDeviceParametersAsync(IList<IBaseParameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                _logger.LogInformation("Deleting Parameter Id: [{Id}] Name: [{Name}]",
                                       parameter.Id,
                                       parameter.Name);

                await _repository.DeleteAsync<ModelDevice>(parameter.Id);
            }

            if (parameters.Count > 0)
            {
                await _repository.SaveChangesAsync();
            }
        }

        private async Task DeleteDevicesAsync(IList<IDevice> devices)
        {
            foreach (var device in devices)
            {
                _logger.LogInformation("Deleting Device Id: [{Id}] Name: [{Name}]",
                                       device.Id,
                                       device.Name);

                await _repository.DeleteAsync<ModelDevice>(device.Id);
            }

            if (devices.Count > 0)
            {
                await _repository.SaveChangesAsync();
            }
        }

        private void DetachDevicesListChange(IObservableList<IDevice> devices)
        {
            devices.ListChanged -= DevicesListChangedAsync;
        }

        private void DetachParameter(IBaseParameter parameter)
        {
            var type = parameter.GetType();

            switch (type)
            {
                case Type _ when type == typeof(BoolParameter):
                    ((BoolParameter)parameter).ValueChanged -= ParameterValueChangedAsync;
                    break;

                case Type _ when type == typeof(FloatParameter):
                    ((FloatParameter)parameter).ValueChanged -= ParameterValueChangedAsync;
                    break;

                case Type _ when type == typeof(IntParameter):
                    ((IntParameter)parameter).ValueChanged -= ParameterValueChangedAsync;
                    break;

                case Type _ when type == typeof(StringParameter):
                    ((StringParameter)parameter).ValueChanged -= ParameterValueChangedAsync;
                    break;

                case Type _ when type == typeof(UIntParameter):
                    ((UIntParameter)parameter).ValueChanged -= ParameterValueChangedAsync;
                    break;
            }
        }

        private void DetachParameters(IChildObservableList<IBaseParameter, IDevice> parameters)
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
                device.Parameters.ListChanged -= ParametersListChangedAsync;

                DetachParameters(device.Parameters);

                DetatchDevice(device);
            }
        }

        private void DetatchDevice(IDevice device)
        {
            device.DeviceChanged -= DeviceChangedAsync;
        }

        private async Task DeviceChangedAsync(object? sender,
                                              DeviceChangedEventArgs e)
        {
            if (sender != null)
            {
                if (sender is IDevice sdkDevice)
                {
                    var modelDevice = await _repository.GetByIdAsync<ModelDevice>(sdkDevice.Id);

                    if (modelDevice != null)
                    {
#warning save new value in list
                        modelDevice.BatteryLevel = sdkDevice.BatteryLevel?.Level;
                        modelDevice.SignalLevel = sdkDevice.SignalLevel?.Level;

#warning TODO send event?
                        await _repository.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogError("Unable to resolve Device for [{Id}]", sdkDevice.Id);
                    }
                }
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
                            await CreateDevicesAsync(e.Items);
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
                            await CreateDeviceParametersAsync(e.Items);
                        }
                        break;

                    case ListChangedAction.Reset:
                        if (e.Items != null)
                        {
                            await DeleteDeviceParametersAsync(e.Items);
                        }
                        break;

                    case ListChangedAction.Remove:
                        if (e.Items != null)
                        {
                            await DeleteDeviceParametersAsync(e.Items);
                        }
                        break;
                }
            }
        }

        private async Task ParameterValueChangedAsync(object sender,
                                                      ParameterValueChangedEventArgs<uint> e)
        {
            await SaveParameterChangeAsync<ModelUIntParameter>(sender, (parameter) =>
            {
                parameter.Value = e.Value;
            });
        }

        private async Task ParameterValueChangedAsync(object sender,
                                                      ParameterValueChangedEventArgs<string> e)
        {
            await SaveParameterChangeAsync<ModelStringParameter>(sender, (parameter) =>
            {
                parameter.Value = e.Value;
            });
        }

        private async Task ParameterValueChangedAsync(object sender,
                                                      ParameterValueChangedEventArgs<int> e)
        {
            await SaveParameterChangeAsync<ModelIntParameter>(sender, (parameter) =>
            {
                parameter.Value = e.Value;
            });
        }

        private async Task ParameterValueChangedAsync(object sender,
                                                      ParameterValueChangedEventArgs<float> e)
        {
            await SaveParameterChangeAsync<ModelFloatParameter>(sender, (parameter) =>
            {
                parameter.Value = e.Value;
            });
        }

        private async Task ParameterValueChangedAsync(object sender,
                                                      ParameterValueChangedEventArgs<bool> e)
        {
            await SaveParameterChangeAsync<ModelBoolParameter>(sender, (parameter) =>
            {
                parameter.Value = e.Value;
            });
        }

        private async Task SaveParameterChangeAsync<T>(object sender,
                                                       Action<T> operation) where T : BaseEntity
        {
            if (sender != null)
            {
                if (sender is IBaseParameter parameter)
                {
                    var modelParameter = await _repository.GetByIdAsync<T>(parameter.Id, false);

                    if (modelParameter != null)
                    {
                        operation(modelParameter);

                        await _repository.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogWarning("Unable to resolve [{Type}] With Id: [{Id}]", typeof(T).Name, parameter.Id);
                    }
                }
            }
        }
    }
}