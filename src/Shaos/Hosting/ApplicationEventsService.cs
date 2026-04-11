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

using Shaos.Services;
using Shaos.Services.Eventing;

namespace Shaos.Hosting
{
    public class ApplicationEventsService : BackgroundService
    {
        private readonly IEventQueue _eventQueue;
        private readonly ILogger<ApplicationEventsService> _logger;
        private readonly IServerSideEventsService _serverSideEventsService;

        public ApplicationEventsService(IEventQueue eventQueue,
                                        ILogger<ApplicationEventsService> logger,
                                        IServerSideEventsService serverSideEventsService)
        {
            _eventQueue = eventQueue;
            _logger = logger;
            _serverSideEventsService = serverSideEventsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _serverSideEventsService.BroadcastEventAsync(await _eventQueue.DequeueAsync(stoppingToken));
            }
        }
    }
}