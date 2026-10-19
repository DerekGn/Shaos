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
        private readonly Mock<IQueueFactory> _mockQueueFactory;
        private readonly ServerSentEventsService _service;
        private readonly List<IQueue<ApplicationEvent>> _applicationEvents;
        private readonly List<IQueue<LogCreatedEvent>> _logEvents;
        private readonly List<IQueue<BaseParameterUpdatedEvent>> _parameterEvents;

        public ServerSentEventsServiceTests()
        {
            _mockQueueFactory = new Mock<IQueueFactory>();
            _applicationEvents = new List<IQueue<ApplicationEvent>>();
            _logEvents = new List<IQueue<LogCreatedEvent>>();
            _parameterEvents = new List<IQueue<BaseParameterUpdatedEvent>>();

            var optionsInstance = new ServerSentEventOptions
            {
            };

            var serverSentEventOptions = Options.Create(optionsInstance);

            _service = new ServerSentEventsService(LoggerFactory!.CreateLogger<ServerSentEventsService>(),
                                                   serverSentEventOptions,
                                                   _mockQueueFactory.Object,
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

        [Fact]
        public async Task TestBroadcastParameterUpdatedEventAsync()
        {
            var queue = new Services.Eventing.Queue<BaseParameterUpdatedEvent>(1);

            _parameterEvents.Add(queue);

            await _service.BroadcastParameterUpdatedEventAsync(new ParameterUpdatedEvent<bool>()
            {
                Value = true,
                CanWrite = true,
                Timestamp = DateTime.UtcNow
            }, TestContext.Current.CancellationToken);

            Assert.Equal(1, queue.Count);

            _parameterEvents.Clear();
        }

        [Fact]
        public async Task TestBroadcastLogEventAsync()
        {
            var queue = new Services.Eventing.Queue<LogCreatedEvent>(1);

            _logEvents.Add(queue);

            await _service.BroadcastLogEventAsync("", TestContext.Current.CancellationToken);

            Assert.Equal(1, queue.Count);

            _logEvents.Clear();
        }

        [Fact]
        public async Task TestStreamApplicationEventsAsync()
        {
            var queue = new Services.Eventing.Queue<ApplicationEvent>(1);

            await queue.EnqueueAsync(new ApplicationEvent(), TestContext.Current.CancellationToken);

            _mockQueueFactory
            .Setup(_ => _.CreateQueue<ApplicationEvent>(It.IsAny<int>()))
            .Returns(queue);

            var result = await _service.StreamApplicationEventsAsync(TestContext.Current.CancellationToken).FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

            Assert.NotNull(result.Data);
            Assert.IsType<ApplicationEvent>(result.Data);
        }

        [Fact]
        public async Task TestStreamLogEventsAsync()
        {
            var queue = new Services.Eventing.Queue<LogCreatedEvent>(1);

            await queue.EnqueueAsync(new LogCreatedEvent(""), TestContext.Current.CancellationToken);

            _mockQueueFactory
            .Setup(_ => _.CreateQueue<LogCreatedEvent>(It.IsAny<int>()))
            .Returns(queue);

            var result = await _service.StreamLogEventsAsync(TestContext.Current.CancellationToken).FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

            Assert.NotNull(result.Data);
            Assert.IsType<LogCreatedEvent>(result.Data);
        }

        [Fact]
        public async Task TestStreamParameterEventsAsync()
        {
            var queue = new Services.Eventing.Queue<BaseParameterUpdatedEvent>(1);

            await queue.EnqueueAsync(new ParameterUpdatedEvent<int>()
            {
                Value = 1
            }, TestContext.Current.CancellationToken);

            _mockQueueFactory
            .Setup(_ => _.CreateQueue<BaseParameterUpdatedEvent>(It.IsAny<int>()))
            .Returns(queue);

            var result = await _service.StreamParameterEventsAsync(TestContext.Current.CancellationToken).FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

            Assert.NotNull(result.Data);
            Assert.IsType<ParameterUpdatedEvent<int>>(result.Data);
        }

        [Fact]
        public async Task TestStreamParameterEventsByIdAsync()
        {
            var queue = new Services.Eventing.Queue<BaseParameterUpdatedEvent>(1);

            await queue.EnqueueAsync(new ParameterUpdatedEvent<int>()
            {
                Value = 1,
                Id  = 1
            }, TestContext.Current.CancellationToken);

            _mockQueueFactory
            .Setup(_ => _.CreateQueue<BaseParameterUpdatedEvent>(It.IsAny<int>()))
            .Returns(queue);

            var result = await _service.StreamParameterEventsByIdAsync( 1, TestContext.Current.CancellationToken).FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

            Assert.NotNull(result.Data);
            Assert.IsType<ParameterUpdatedEvent<int>>(result.Data);
        }
    }
}
