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

using Microsoft.AspNetCore.SignalR;
using Shaos.Hubs;
using Shaos.Services.Eventing;
using Shaos.Services.Hosting;

namespace Shaos.Hosting
{
    public partial class PlotPublishBackgroundService : BaseBackgroundService
    {
        private readonly IDeviceEventQueue _deviceEventQueue;
        private readonly IHubContext<PlotHub, IPlotHub> _hubContext;
        private readonly Dictionary<int, List<string>> _subscriptions;

        public PlotPublishBackgroundService(ILogger<PlotPublishBackgroundService> logger,
                                            IHubContext<PlotHub, IPlotHub> hubContext,
                                            IDeviceEventQueue deviceEventQueue) : base(logger)
        {
            _hubContext = hubContext;
            _deviceEventQueue = deviceEventQueue;
            _subscriptions = new Dictionary<int, List<string>>();
        }

        protected override async Task ExecuteInternalAsync(CancellationToken stoppingToken)
        {
            var @event = await _deviceEventQueue.DequeueAsync(stoppingToken);
            var type = @event.GetType();

            if (type == typeof(DeviceParameterSubscriptionEvent))
            {
                HandleDeviceParameterSubscriptionEvent((DeviceParameterSubscriptionEvent)@event);
            }
            else
            {
                await HandleDeviceParameterUpdatedEventAsync(@event);
            }
        }

        private void HandleDeviceParameterSubscriptionEvent(DeviceParameterSubscriptionEvent @event)
        {
            if (@event.State == DeviceSubscriptionState.Subscribe)
            {
                if (!_subscriptions.TryGetValue(@event.ParameterId,
                                                out List<string>? userIds))
                {
                    userIds = [];
                    _subscriptions.Add(@event.ParameterId, userIds);
                }

                if (!userIds.Contains(@event.UserIdentifier))
                {
                    userIds.Add(@event.UserIdentifier);
                }
                else
                {
                    Logger.LogWarning("User subscription exists. Parameter: [{Id}] User: [{User}]",
                                      @event.ParameterId,
                                      @event.UserIdentifier);
                }
            }
            else
            {
                if (_subscriptions.TryGetValue(@event.ParameterId,
                                               out List<string>? userIds))
                {
                    if (userIds.Contains(@event.UserIdentifier))
                    {
                        userIds.Remove(@event.UserIdentifier);
                    }
                    else
                    {
                        Logger.LogWarning("User subscription does not exist. Parameter: [{Id}] User: [{User}]",
                                          @event.ParameterId,
                                          @event.ParameterId);
                    }
                }
                else
                {
                    Logger.LogWarning("User subscription does not exist. Parameter: [{Id}] User: [{User}]",
                                      @event.ParameterId,
                                      @event.ParameterId);
                }
            }
        }

        private async Task HandleDeviceParameterUpdatedEventAsync(BaseDeviceEvent @event)
        {
            if (_subscriptions.TryGetValue(@event.ParameterId, out var subscriptionUsers))
            {
                switch (@event)
                {
                    case DeviceParameterUpdatedEvent<bool> parameterValue:
                        await _hubContext.Clients.Users(subscriptionUsers).UpdateAsync(parameterValue.Value,
                                                                                       parameterValue.Timestamp);
                        break;

                    case DeviceParameterUpdatedEvent<float> parameterValue:
                        await _hubContext.Clients.Users(subscriptionUsers).UpdateAsync(parameterValue.Value,
                                                                                       parameterValue.Timestamp);
                        break;

                    case DeviceParameterUpdatedEvent<int> parameterValue:
                        await _hubContext.Clients.Users(subscriptionUsers).UpdateAsync(parameterValue.Value,
                                                                                       parameterValue.Timestamp);
                        break;

                    case DeviceParameterUpdatedEvent<string> parameterValue:
                        await _hubContext.Clients.Users(subscriptionUsers).UpdateAsync(parameterValue.Value,
                                                                                       parameterValue.Timestamp);
                        break;

                    case DeviceParameterUpdatedEvent<uint> parameterValue:
                        await _hubContext.Clients.Users(subscriptionUsers).UpdateAsync(parameterValue.Value,
                                                                                       parameterValue.Timestamp);
                        break;
                }
            }
            else
            {
                Logger.LogWarning("No parameter subscriptions exist for parameter: [{Id}]",
                                  @event.ParameterId);
            }
        }

        //[LoggerMessage(Level = LogLevel.Warning, Message = "User subscription exists. Parameter: [{id}] User: [{user}]")]
        //private partial void LogUserSubscriptionExists(int id,
        //                                               string user);
    }
}