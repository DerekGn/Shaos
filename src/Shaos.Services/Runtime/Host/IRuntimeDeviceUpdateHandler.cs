using Shaos.Repository.Models;
using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;

namespace Shaos.Services.Runtime.Host
{
    public interface IRuntimeDeviceUpdateHandler
    {
        Task CreateDeviceParametersAsync(int id, IList<IBaseParameter> parameters);

        Task CreateDevicesAsync(int id, IList<IDevice> devices);

        Task DeleteDeviceParametersAsync(IList<IBaseParameter> items);

        Task DeleteDevicesAsync(IList<IDevice> items);

        Task DeviceBatteryLevelUpdateAsync(IDevice device, BatteryLevelChangedEventArgs e);

        Task DeviceSignalLevelUpdateAsync(IDevice device, SignalLevelChangedEventArgs e);

        Task SaveParameterChangeAsync<T>(IBaseParameter parameter, Action<T> operation) where T : BaseEntity;
    }
}