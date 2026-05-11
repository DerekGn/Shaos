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
using Microsoft.Extensions.Options;
using Shaos.Services.Eventing;
using Shaos.Services.Extensions;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;

namespace Shaos.Services
{
    /// <summary>
    /// A <see cref="IServerSideEventsService"/> implementation
    /// </summary>
    public class ServerSideEventsService : IServerSideEventsService
    {
        private readonly List<IEventQueue> _eventQueues;
        private readonly ILogger<ServerSideEventsService> _logger;
        private readonly IOptions<ServerSideEventOptions> _options;
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// Create an instance of a <see cref="ServerSideEventsService"/>
        /// </summary>
        /// <param name="logger">The logger instance for this service</param>
        /// <param name="options">The options instance for this service</param>
        public ServerSideEventsService(ILogger<ServerSideEventsService> logger,
                                       IOptions<ServerSideEventOptions> options)
        {
            _logger = logger;
            _options = options;
            _semaphore = new SemaphoreSlim(1);
            _eventQueues = new List<IEventQueue>();
        }

        /// <inheritdoc/>
        public async Task BroadcastEventAsync(BaseEvent baseEvent)
        {
            await AccessClientQueuesAsync(() =>
            {
                foreach (var queue in _eventQueues)
                {
                    queue.EnqueueAsync(baseEvent);
                }
            });
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<SseItem<ApplicationEvent>> StreamApplicationEventsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<SseItem<BaseParameterUpdatedEvent>> StreamParameterEventsAsync(CancellationToken cancellationToken)
        {
            return StreamParameterEventsAsync((parameterUpdateEvent) =>
            {
                return true;
            }, cancellationToken);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<SseItem<BaseParameterUpdatedEvent>> StreamParameterEventsByIdAsync(int id,
                                                                                                   CancellationToken cancellationToken)
        {
            return StreamParameterEventsAsync((parameterUpdateEvent) =>
            {
                return parameterUpdateEvent.Id == id;
            }, cancellationToken);
        }

        private async Task AccessClientQueuesAsync(Action action)
        {
            await _semaphore.WaitAsync();

            try
            {
                action();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async IAsyncEnumerable<SseItem<BaseParameterUpdatedEvent>> StreamParameterEventsAsync(Func<BaseParameterUpdatedEvent, bool> filterEvent,
                                                                                                      [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            EventQueue? eventQueue = null;

            try
            {
                eventQueue = new EventQueue(_options.Value.EventQueueCapacity);

                await AccessClientQueuesAsync(async () =>
                {
                    _eventQueues.Add(eventQueue);
                });

                while (!cancellationToken.IsCancellationRequested)
                {
                    var baseEvent = await eventQueue.DequeueAsync(cancellationToken);

                    if (baseEvent is BaseParameterUpdatedEvent parameterEvent && filterEvent(parameterEvent))
                    {
                        yield return new SseItem<BaseParameterUpdatedEvent>(parameterEvent,
                                                                            baseEvent.GetEventName())
                        {
                            EventId = Guid.NewGuid().ToString(),
                            ReconnectionInterval = _options.Value.ReconnectInterval
                        };
                    }
                }

                _logger.EventStreamingComplete();
            }
            finally
            {
                if (eventQueue is not null)
                {
                    await AccessClientQueuesAsync(async () =>
                    {
                        _eventQueues.Remove(eventQueue);
                    });
                }
            }
        }
    }
}