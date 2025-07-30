using Microsoft.Extensions.Logging;
using Shaos.Repository;
using Shaos.Sdk;
using Shaos.Sdk.Collections.Generic;
using Shaos.Sdk.Devices;
using Shaos.Services.Extensions;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

            plugIn.Devices.ListChanged += DevicesListChanged;
        }

        /// <inheritdoc/>
        public void Detach(IPlugIn? plugIn)
        {
            ArgumentNullException.ThrowIfNull(plugIn);

            DetachPlugInDevice(plugIn.Devices);

            plugIn.Devices.ListChanged -= DevicesListChanged;
        }

        private void AttachPlugInDevice(ObservableList<SdkDevice> devices)
        {
            foreach (var device in devices)
            {
                device.Parameters.ListChanged += ParametersListChanged;

                device.DeviceChanged += DeviceChanged;
            }
        }

        private void DetachPlugInDevice(ObservableList<SdkDevice> devices)
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

        private async Task DevicesListChanged(object sender, ListChangedEventArgs<SdkDevice> e)
        {
            if (sender != null)
            {
                switch (e.Action)
                {
                    case ListChangedAction.Add:
                        break;
                    case ListChangedAction.Reset:
                        break;
                    case ListChangedAction.Remove:
                        break;
                        //case NotifyCollectionChangedAction.Add:
                        //    if (e.NewItems != null)
                        //    {
                        //        foreach (SdkDevice device in e.NewItems)
                        //        {
                        //            var modelDevice = device.ToModel();

                        //            await _repository.AddAsync(modelDevice);

                        //            await _repository.SaveChangesAsync();

                        //            device.Id = modelDevice.Id;
                        //        }
                        //    }

                        //    break;

                        //case NotifyCollectionChangedAction.Remove:
                        //    break;

                        //case NotifyCollectionChangedAction.Replace:
                        //    break;

                        //case NotifyCollectionChangedAction.Move:
                        //    break;

                        //case NotifyCollectionChangedAction.Reset:
                        //    break;

                        //default:
                        //    break;
                }
            }
        }

        private async Task ParametersListChanged(object sender, ListChangedEventArgs<Sdk.Devices.Parameters.BaseParameter> e)
        {
            if (sender != null)
            {
                switch (e.Action)
                {
                    case ListChangedAction.Add:
                        break;
                    case ListChangedAction.Reset:
                        break;
                    case ListChangedAction.Remove:
                        break;
                        //case NotifyCollectionChangedAction.Add:
                        //    if (e.NewItems != null)
                        //    {
                        //        foreach (SdkDevice device in e.NewItems)
                        //        {
                        //            var modelDevice = device.ToModel();

                        //            await _repository.AddAsync(modelDevice);

                        //            await _repository.SaveChangesAsync();

                        //            device.Id = modelDevice.Id;
                        //        }
                        //    }
                        //    break;

                        //case NotifyCollectionChangedAction.Remove:
                        //    break;

                        //case NotifyCollectionChangedAction.Replace:
                        //    break;

                        //case NotifyCollectionChangedAction.Move:
                        //    break;

                        //case NotifyCollectionChangedAction.Reset:
                        //    break;

                        //default:
                        //    break;
                }
            }
        }
    }
}