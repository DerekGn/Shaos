using Microsoft.Extensions.Logging;
using Shaos.Repository;
using Shaos.Sdk;
using Shaos.Sdk.Devices;
using Shaos.Services.Extensions;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using ModelDevice = Shaos.Repository.Models.Devices.Device;
using SdkDevice = Shaos.Sdk.Devices.Device;

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

            plugIn.Devices.CollectionChanged += DevicesCollectionChanged;
        }

        /// <inheritdoc/>
        public void Detach(IPlugIn? plugIn)
        {
            ArgumentNullException.ThrowIfNull(plugIn);

            DetachPlugInDevice(plugIn.Devices);

            plugIn.Devices.CollectionChanged -= DevicesCollectionChanged;
        }

        private void AttachPlugInDevice(ObservableCollection<SdkDevice> devices)
        {
            foreach (var device in devices)
            {
                device.Parameters.CollectionChanged += ParametersCollectionChanged;

                device.DeviceChanged += DeviceChanged;
            }
        }

        private void DetachPlugInDevice(ObservableCollection<SdkDevice> devices)
        {
            foreach (var device in devices)
            {
                device.Parameters.CollectionChanged -= ParametersCollectionChanged;

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

        private void DevicesCollectionChanged(object? sender,
                                              NotifyCollectionChangedEventArgs e)
        {
            if (sender != null)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if(e.NewItems != null)
                        {
                            foreach (SdkDevice device in e.NewItems)
                            {
                                _repository.AddAsync(device.ToModel());

                                device.Id = device.Id;
                            }
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        break;

                    case NotifyCollectionChangedAction.Move:
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        break;

                    default:
                        break;
                }
            }
        }

        private void ParametersCollectionChanged(object? sender,
                                                 NotifyCollectionChangedEventArgs e)
        {
            if (sender != null)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        break;

                    case NotifyCollectionChangedAction.Move:
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        break;

                    default:
                        break;
                }
            }
        }
    }
}