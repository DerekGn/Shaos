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

using Microsoft.Extensions.Logging;
using Shaos.Services.Eventing;
using Shaos.Services.Extensions;
using System.Collections.Concurrent;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;

namespace Shaos.Services
{
    public class ServerSideEventsService : IServerSideEventsService
    {
        private readonly ILogger<ServerSideEventsService> _logger;
        private readonly ConcurrentDictionary<string, IEventQueue> _subscriberQueues;

        public ServerSideEventsService(ILogger<ServerSideEventsService> logger)
        {
            _logger = logger;
            _subscriberQueues = new ConcurrentDictionary<string, IEventQueue>();
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<SseItem<BaseEvent>> StreamEventsAsync(string id,
                                                                            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            try
            {
                _subscriberQueues.TryAdd(id, new EventQueue(10));

                while (!cancellationToken.IsCancellationRequested)
                {
                    var baseEvent = await _subscriberQueues[id].DequeueAsync(cancellationToken);

                    if(baseEvent is not null)
                    {
                        yield return new SseItem<BaseEvent>(baseEvent, baseEvent.GetEventName())
                        {
                            EventId = Guid.NewGuid().ToString(),
                            ReconnectionInterval = TimeSpan.FromMinutes(1)
                        };
                    }
                }

                _logger.EventStreamingComplete(id);
            }
            finally
            {
                _subscriberQueues.TryRemove(id, out var _);
            }
        }

        /// <inheritdoc/>
        public async Task BroadcastEventAsync(BaseEvent baseEvent)
        {
            foreach (var item in _subscriberQueues.Values)
            {
                await item.EnqueueAsync(baseEvent);
            }
        }
    }
}