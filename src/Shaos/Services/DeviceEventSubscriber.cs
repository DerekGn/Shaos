
using System.Collections.Concurrent;

namespace Shaos.Services
{
    public class DeviceEventSubscriber : IDeviceEventSubscriber
    {
        private ConcurrentDictionary<int, List<string>> _subscriptions;

        public DeviceEventSubscriber()
        {
            _subscriptions = new ConcurrentDictionary<int, List<string>>();
        }

        /// <inheritdoc/>
        public async Task SubscribeDeviceParameterAsync(string userIdentifier,
                                                        int id)
        {
            if(_subscriptions.TryGetValue(id, out List<string>? userIds))
            {
                userIds.Add(userIdentifier);
            }
            else
            {
                if(!_subscriptions.TryAdd(id, [userIdentifier]))
                {
                    _subscriptions[id].Add(userIdentifier);
                }
            }

            // start subscription.....
        }

        /// <inheritdoc/>
        public async Task UnsubscribeDeviceParameterAsync(string userIdentifier,
                                                          int id)
        {
            if (_subscriptions.TryGetValue(id, out List<string>? userIds))
            {
                userIds.Remove(userIdentifier);
            }
        }
    }
}
