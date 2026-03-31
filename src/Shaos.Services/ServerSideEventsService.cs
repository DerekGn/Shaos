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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shaos.Services.Eventing;
using Shaos.Services.Extensions;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Timers;

namespace Shaos.Services
{
    public class ServerSideEventsService : IServerSideEventsService
    {
        private readonly ConcurrentDictionary<HttpContext, bool> _clientContexts = new();
        private readonly SemaphoreSlim _semaphore;
        private readonly System.Timers.Timer _timer;
        private readonly ILogger<ServerSideEventsService> _logger;
        private readonly IOptions<ServerSideEventOptions> _options;
        private DateTime _lastEventPush;

        public ServerSideEventsService(ILogger<ServerSideEventsService> logger,
                                       IOptions<ServerSideEventOptions> options)
        {
            _semaphore = new SemaphoreSlim(1);
            _timer = new System.Timers.Timer(options.Value.HeartBeatInterval);
            _timer.Elapsed += OnTimedEvent;
            _timer.Enabled = true;
            _logger = logger;
            _options = options;
        }

        /// <inheritdoc />
        public void AddContext(HttpContext context)
        {
            _clientContexts.TryAdd(context, true);
        }

        /// <inheritdoc />
        public async Task BroadcastEventAsync(BaseEvent baseEvent)
        {
            await ExecuteConnectedClientsOperationAsync(async (context) =>
            {
                await context.Response.WriteAsync(JsonSerializer.Serialize<BaseEvent>(baseEvent));
                await context.Response.Body.FlushAsync();
            });
        }

        /// <inheritdoc />
        public void RemoveContext(HttpContext context)
        {
            _clientContexts.TryRemove(context, out _);
        }

        private async Task ExecuteConnectedClientsOperationAsync(Func<HttpContext, Task> operation)
        {
            try
            {
                await _semaphore.WaitAsync();

                var disconnectedClients = new List<HttpContext>();

                foreach (var context in _clientContexts.Keys)
                {
                    if (context.RequestAborted.IsCancellationRequested)
                    {
                        disconnectedClients.Add(context);
                    }
                    else
                    {
                        await operation(context);
                    }
                }

                foreach (var context in disconnectedClients)
                {
                    RemoveContext(context);
                }

                _lastEventPush = DateTime.UtcNow;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            await ExecuteConnectedClientsOperationAsync(async (context) =>
            {
                if(DateTime.UtcNow.Subtract(_lastEventPush) < _options.Value.HeartBeatInterval)
                {
                    _logger.LogHeartbeatSend(context.Connection.RemoteIpAddress);
                    
                    await context.Response.WriteAsync(": heartbeat");
                    await context.Response.Body.FlushAsync();
                }
            });
        }
    }
}