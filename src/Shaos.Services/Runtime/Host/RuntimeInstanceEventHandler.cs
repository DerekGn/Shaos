using Microsoft.Extensions.Logging;
using Shaos.Repository;
using Shaos.Sdk;
using Shaos.Sdk.Devices;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Shaos.Services.Runtime.Host
{
    /// <summary>
    /// The runtime instance event handler
    /// </summary>
    public class RuntimeInstanceEventHandler : IRuntimeInstanceEventHandler
    {
        private readonly ILogger<RuntimeInstanceEventHandler> _logger;
        private readonly IPlugInRepository _plugInRepository;

        /// <summary>
        /// Create an instance of a <see cref="IRuntimeInstanceEventHandler"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance</param>
        /// <param name="plugInRepository">The <see cref="IPlugInRepository"/></param>
        public RuntimeInstanceEventHandler(ILogger<RuntimeInstanceEventHandler> logger,
                                           IPlugInRepository plugInRepository)
        {
            _logger = logger;
            _plugInRepository = plugInRepository;
        }

        /// <inheritdoc/>
        public void Attach(IPlugIn? plugIn)
        {
            ArgumentNullException.ThrowIfNull(plugIn);

            AttachPlugInDevice(plugIn.Devices);

            plugIn.Devices.CollectionChanged += Devices_CollectionChanged;
        }

        /// <inheritdoc/>
        public void Detach(IPlugIn? plugIn)
        {
            ArgumentNullException.ThrowIfNull(plugIn);

            DetachPlugInDevice(plugIn.Devices);

            plugIn.Devices.CollectionChanged -= Devices_CollectionChanged;
        }

        private void AttachPlugInDevice(ObservableCollection<Device> devices)
        {
            foreach (var device in devices)
            {
                device.Parameters.CollectionChanged += Parameters_CollectionChanged;

                device.DeviceChanged += Device_DeviceChanged;
            }
        }

        private void DetachPlugInDevice(ObservableCollection<Device> devices)
        {
            foreach (var device in devices)
            {
                device.Parameters.CollectionChanged -= Parameters_CollectionChanged;

                device.DeviceChanged += Device_DeviceChanged;
            }
        }

        private void Device_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
        }

        private void Devices_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
        }

        private void Parameters_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
        }
    }
}