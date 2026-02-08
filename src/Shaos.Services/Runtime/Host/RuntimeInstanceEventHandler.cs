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
using Shaos.Sdk;
using Shaos.Sdk.Collections.Generic;
using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;

namespace Shaos.Services.Runtime.Host
{
    /// <summary>
    /// The runtime instance event handler
    /// </summary>
    public partial class RuntimeInstanceEventHandler : IRuntimeInstanceEventHandler
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

        internal void AttachDevicesListChange(IChildObservableList<IPlugIn, IDevice> devices)
        {
            LogAttachingDevicesListChangedHandler(devices.Parent.Id);

            devices.ListChanged += DevicesListChangedAsync;
        }

        internal void AttachParametersListChanged(IChildObservableList<IDevice, IBaseParameter> parameters)
        {
            LogAttachParametersListChangedHandler(parameters.Parent.Id,
                                                  parameters.Parent.Name);

            parameters.ListChanged += ParametersListChangedAsync;
        }

        internal void DetachParametersListChanged(IChildObservableList<IDevice, IBaseParameter> parameters)
        {
            LogDetachParametersListChangedHandler(parameters.Parent.Id,
                                                  parameters.Parent.Name);

            parameters.ListChanged -= ParametersListChangedAsync;
        }

        internal async Task ParameterValueChangedAsync(object sender,
                                                       ParameterValueChangedEventArgs<uint> e)
        {
            await ValidateParameterChangeAsync(sender, async (parameter) =>
            {
                await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(parameter.Id,
                                                                           e.Value,
                                                                           e.TimeStamp);
            });
        }

        internal async Task ParameterValueChangedAsync(object sender,
                                                       ParameterValueChangedEventArgs<string> e)
        {
            await ValidateParameterChangeAsync(sender, async (parameter) =>
            {
                await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(parameter.Id,
                                                                           e.Value,
                                                                           e.TimeStamp);
            });
        }

        internal async Task ParameterValueChangedAsync(object sender,
                                                       ParameterValueChangedEventArgs<int> e)
        {
            await ValidateParameterChangeAsync(sender, async (parameter) =>
            {
                await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(parameter.Id,
                                                                           e.Value,
                                                                           e.TimeStamp);
            });
        }

        internal async Task ParameterValueChangedAsync(object sender,
                                                       ParameterValueChangedEventArgs<float> e)
        {
            await ValidateParameterChangeAsync(sender, async (parameter) =>
            {
                await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(parameter.Id,
                                                                           e.Value,
                                                                           e.TimeStamp);
            });
        }

        internal async Task ParameterValueChangedAsync(object sender,
                                                       ParameterValueChangedEventArgs<bool> e)
        {
            await ValidateParameterChangeAsync(sender, async (parameter) =>
            {
                await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(parameter.Id,
                                                                           e.Value,
                                                                           e.TimeStamp);
            });
        }

        private static async Task ExecuteDeviceOperationAsync(object sender,
                                                              Func<IDevice, Task> operation)
        {
            if (sender is IDevice device)
            {
                await operation(device);
            }
        }

        private void AttachDeviceAndParameters(IDevice device)
        {
            AttachParametersListChanged(device.Parameters);

            AttachParameters([.. device.Parameters]);
        }

        private void AttachParameter(IBaseParameter parameter)
        {
            LogAttachingParameterEventHandler(parameter.Id,
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
            LogDetachParametersChangedHandler(parameter.Id,
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
            foreach (var parameters in devices.Select(_ => _.Parameters))
            {
                DetachParametersListChanged(parameters);

                DetachParameters(parameters.ToList());
            }
        }

        private async Task DevicesListChangedAsync(object sender,
                                                   ListChangedEventArgs<IDevice> e)
        {
            if (sender is IChildObservableList<IPlugIn, IDevice> devices)
            {
                if (e.Items != null)
                {
                    switch (e.Action)
                    {
                        case ListChangedAction.Add:

                            await _runtimeDeviceUpdateHandler.CreateDevicesAsync(devices.Parent.Id,
                                                                                 e.Items);

                            foreach (var device in e.Items)
                            {
                                AttachDeviceAndParameters(device);
                            }

                            break;

                        case ListChangedAction.Reset:
                            await _runtimeDeviceUpdateHandler.DeleteDevicesAsync(e.Items.Select(_ => _.Id));

                            break;

                        case ListChangedAction.Remove:
                            await _runtimeDeviceUpdateHandler.DeleteDevicesAsync(e.Items.Select(_ => _.Id));

                            break;
                    }
                }
                else
                {
                    LogEventItemsEmpty();
                }
            }
            else
            {
                LogInvalidType(sender.GetType());
            }
        }

        [LoggerMessage(Level = LogLevel.Debug, Message = "Attaching device signal level and battery level event handler for Device: [{id}] Name: [{name}]")]
        private partial void LogAttachingDeviceSignalAndBatteryHandlers(int id,
                                                                        string name);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Attaching device list event handler for PlugIn Id: [{id}]")]
        private partial void LogAttachingDevicesListChangedHandler(int id);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Attaching event handler for parameter Id: [{id}] Name: [{name}]")]
        private partial void LogAttachingParameterEventHandler(int id, string? name);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Attaching parameter list event handler for PlugIn Id: [{id}] Name: [{name}]")]
        private partial void LogAttachParametersListChangedHandler(int id,
                                                                   string name);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Detaching device signal level and battery level event handler for Device: [{id}] Name: [{name}]")]
        private partial void LogDetachingDeviceSignalAndBatteryHandlers(int id,
                                                                        string name);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Detaching event handler for parameter Id: [{id}] Name: [{name}]")]
        private partial void LogDetachParametersChangedHandler(int id, string? name);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Detaching parameter list event handler for PlugIn Id: [{id}] Name: [{name}]")]
        private partial void LogDetachParametersListChangedHandler(int id,
                                                                   string name);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Event items collection empty")]
        private partial void LogEventItemsEmpty();

        [LoggerMessage(Level = LogLevel.Warning, Message = "Sender is invalid type: [{type}]")]
        private partial void LogInvalidType(Type type);

        private async Task ParametersListChangedAsync(object sender,
                                                      ListChangedEventArgs<IBaseParameter> e)
        {
            if (sender is IChildObservableList<IDevice, IBaseParameter> deviceParameters)
            {
                if (e.Items != null)
                {
                    switch (e.Action)
                    {
                        case ListChangedAction.Add:
                            await _runtimeDeviceUpdateHandler.CreateDeviceParametersAsync(deviceParameters.Parent.Id,
                                                                                          e.Items);

                            AttachParameters([.. e.Items]);
                            break;

                        case ListChangedAction.Remove:
                        case ListChangedAction.Reset:
                            DetachParameters(e.Items);

                            await _runtimeDeviceUpdateHandler.DeleteDeviceParametersAsync(e.Items.Select(_ => _.Id));
                            break;
                    }
                }
                else
                {
                    LogEventItemsEmpty();
                }
            }
            else
            {
                LogInvalidType(sender.GetType());
            }
        }

        private async Task ValidateParameterChangeAsync(object sender,
                                                        Func<IBaseParameter, Task> operation)
        {

            if (sender is IBaseParameter parameter)
            {
                await operation(parameter);
            }
            else
            {
                LogInvalidType(sender.GetType());
            }
        }
    }
}