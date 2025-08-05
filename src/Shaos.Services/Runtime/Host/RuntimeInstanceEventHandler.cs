using Microsoft.Extensions.Logging;
using Shaos.Repository;
using Shaos.Sdk;
using Shaos.Sdk.Collections.Generic;
using Shaos.Sdk.Devices;
using Shaos.Services.Extensions;

using SdkDevice = Shaos.Sdk.Devices.Device;
using SdkIBaseParameter = Shaos.Sdk.Devices.Parameters.IBaseParameter;

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
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance</param>
        /// <param name="repository">The <see cref="IRepository"/></param>
        public RuntimeInstanceEventHandler(ILogger<RuntimeInstanceEventHandler> logger,
                                           IRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        /// <inheritdoc/>
        public void Attach(IPlugIn? plugIn)
        {
            ArgumentNullException.ThrowIfNull(plugIn);

            AttachPlugInDevice(plugIn.Devices);

            plugIn.Devices.ListChanged += DevicesListChanged;
        }

        /// <inheritdoc/>
        public void Detach(IPlugIn? plugIn)
        {
            ArgumentNullException.ThrowIfNull(plugIn);

            DetachPlugInDevice(plugIn.Devices);

            plugIn.Devices.ListChanged -= DevicesListChanged;
        }

        private void AttachPlugInDevice(IObservableList<SdkDevice> devices)
        {
            foreach (var device in devices)
            {
                device.Parameters.ListChanged += ParametersListChanged;

                device.DeviceChanged += DeviceChanged;
            }
        }

        private async Task CreateDeviceParametersAsync(IList<SdkIBaseParameter> items)
        {
            foreach (var item in items)
            {
                var modelParameter = item.ToModel();

                await _repository.AddAsync(modelParameter!);

                await _repository.SaveChangesAsync();
            }
        }

        private async Task CreateDevicesAsync(IList<SdkDevice> devices)
        {
            foreach (SdkDevice device in devices)
            {
                var modelDevice = device.ToModel();

                await _repository.AddAsync(modelDevice);

                await _repository.SaveChangesAsync();

                device.SetId(modelDevice.Id);
            }
        }

        private async Task DeleteDeviceParametersAsync(IList<SdkIBaseParameter> items)
        {
        }

        private async Task DeleteDevicesAsync(IList<SdkDevice> items)
        {
        }

        private void DetachPlugInDevice(IObservableList<SdkDevice> devices)
        {
            foreach (var device in devices)
            {
                device.Parameters.ListChanged -= ParametersListChanged;

                device.DeviceChanged += DeviceChanged;
            }
        }

        private void DeviceChanged(object? sender,
                                   DeviceChangedEventArgs e)
        {
            if (sender != null)
            {
            }
        }

        private async Task DevicesListChanged(object sender,
                                              ListChangedEventArgs<SdkDevice> e)
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

        private async Task ParametersListChanged(object sender,
                                                 ListChangedEventArgs<SdkIBaseParameter> e)
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
    }
}