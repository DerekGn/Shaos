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
using Moq;
using Shaos.Services.Eventing;
using Shaos.Services.Eventing.Application;
using Shaos.Services.Eventing.Factories;
using Shaos.Services.Eventing.Logging;
using Shaos.Services.Eventing.Parameter;
using Xunit;

namespace Shaos.Services.UnitTests
{
    public class ServerSentEventsServiceTests : BaseServiceTests
    {
        private readonly Mock<IQueueFactory> _mockEventQueueFactory;
        private readonly ServerSentEventsService _service;
        private readonly List<IQueue<ApplicationEvent>> _applicationEvents;
        private readonly List<IQueue<LogCreatedEvent>> _logEvents;
        private readonly List<IQueue<BaseParameterUpdatedEvent>> _parameterEvents;

        public ServerSentEventsServiceTests()
        {
            _mockEventQueueFactory = new Mock<IQueueFactory>();
            _applicationEvents = new List<IQueue<ApplicationEvent>>();
            _logEvents = new List<IQueue<LogCreatedEvent>>();
            _parameterEvents = new List<IQueue<BaseParameterUpdatedEvent>>();

            var optionsInstance = new ServerSentEventOptions
            {
            };

            var serverSentEventOptions = Options.Create(optionsInstance);

            _service = new ServerSentEventsService(LoggerFactory!.CreateLogger<ServerSentEventsService>(),
                                                   serverSentEventOptions,
                                                   _mockEventQueueFactory.Object,
                                                   _applicationEvents,
                                                   _logEvents,
                                                   _parameterEvents);
        }

        [Fact]
        public async Task TestBroadcastApplicationEventAsync()
        {
            var queue = new Services.Eventing.Queue<ApplicationEvent>(1);

            _applicationEvents.Add(queue);

            await _service.BroadcastApplicationEventAsync(new ApplicationEvent(),
                                                          TestContext.Current.CancellationToken);

            Assert.Equal(1, queue.Count);

            _applicationEvents.Clear();
        }

        public async Task Test()
        {
            await _service.BroadcastParameterUpdatedEventAsync(new ParameterUpdatedEvent<bool>()
            {
                Value = true,
                CanWrite = true,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
