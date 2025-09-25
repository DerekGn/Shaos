
namespace Shaos.Services
{
    public interface IDeviceEventSubscriber
    {
        Task SubscribeDeviceParameterAsync(string userIdentifier, int id);
        Task UnsubscribeDeviceParameterAsync(string userIdentifier, int id);
    }
}
