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
using Shaos.Services.Eventing;

namespace Shaos.Hubs
{
    public partial class PlotHub : Hub<IPlotHub>
    {
        private readonly IDeviceEventQueue _deviceEventQueue;
        private readonly ILogger<PlotHub> _logger;

        public PlotHub(ILogger<PlotHub> logger,
                       IDeviceEventQueue deviceEventQueue)
        {
            _logger = logger;
            _deviceEventQueue = deviceEventQueue;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("User: [{Id}] Connected. Connection Id: [{ConnectionId}]",
                                   Context.UserIdentifier,
                                   Context.ConnectionId);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("User: [{Id}] Disconnection. Connection Id: [{ConnectionId}]",
                                   Context.UserIdentifier,
                                   Context.ConnectionId);

            if (exception != null)
            {
                _logger.LogError(exception,
                                 "User: [{Id}] Disconnection. Connection Id: [{ConnectionId}]",
                                 Context.UserIdentifier,
                                 Context.ConnectionId);
            }

            return base.OnDisconnectedAsync(exception);
        }

        [HubMethodName("Start")]
        public async Task StartAsync(int id)
        {
            LogStartParameterPlot(id,
                                  Context.UserIdentifier);

            await DeviceParameterSubscription(id, DeviceSubscriptionState.Subscribe);
        }

        [HubMethodName("Stop")]
        public async Task StopAsync(int id)
        {
            LogStopParameterPlot(id,
                                 Context.UserIdentifier);

            await DeviceParameterSubscription(id, DeviceSubscriptionState.Unsubscribe);
        }

        private async Task DeviceParameterSubscription(int id, DeviceSubscriptionState state)
        {
            await _deviceEventQueue.EnqueueAsync(new DeviceParameterSubscriptionEvent()
            {
                ParameterId = id,
                State = state,
                UserIdentifier = Context.UserIdentifier!
            });
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Starting Plot Parameter Id: [{id}] for User Id: [{userIdentifier}]")]
        private partial void LogStartParameterPlot(int id, string? userIdentifier);

        [LoggerMessage(Level = LogLevel.Information, Message = "Stopping Plot Parameter Id: [{id}] for User Id: [{userIdentifier}]")]
        private partial void LogStopParameterPlot(int id, string? userIdentifier);
    }
}