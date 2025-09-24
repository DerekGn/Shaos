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
using Shaos.Services;

namespace Shaos.Hubs
{
    public class PlotHub : Hub<IPlotHub>
    {
        private readonly ILogger<PlotHub> _logger;
        private readonly IDeviceEventSubscriber _deviceEventSubscriber;

        public PlotHub(ILogger<PlotHub> logger,
                       IDeviceEventSubscriber deviceEventSubscriber)
        {
            _logger = logger;
            _deviceEventSubscriber = deviceEventSubscriber;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("User: {User} Connected. Connection Id: [{ConnectionId}]",
                                   Context.UserIdentifier,
                                   Context.ConnectionId);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("User: {User} Disconnected. Connection Id: [{ConnectionId}]",
                                   Context.UserIdentifier,
                                   Context.ConnectionId);

            if (exception != null)
            {
                _logger.LogWarning(exception, "An exception occurred connection disconnected");
            }

            return base.OnDisconnectedAsync(exception);
        }

        [HubMethodName("Start")]
        public async Task StartAsync(int id)
        {
            _logger.LogInformation("Starting Plot Parameter Id: [{Id}]", id);

            await _deviceEventSubscriber.SubscribeDeviceParameterAsync(Context.UserIdentifier, id);
        }

        [HubMethodName("Stop")]
        public async Task StopAsync(int id)
        {
            _logger.LogInformation("Stopping Plot Parameter Id: [{Id}]", id);

            await _deviceEventSubscriber.UnsubscribeDeviceParameterAsync(Context.UserIdentifier, id);
        }

        public async Task UpdateAsync() => await Clients.All.UpdateAsync();
    }
}