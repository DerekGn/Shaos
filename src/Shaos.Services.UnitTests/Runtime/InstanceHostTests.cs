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
using Shaos.Sdk;
using Shaos.Services.Runtime;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Shared.Tests;
using Shaos.Services.UnitTests.Fixtures;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.Runtime
{
    public class InstanceHostTests : BaseTests, IClassFixture<TestFixture>, IDisposable
    {
        private const int WaitDelay = 10;
        private const int WaitItterations = 500;

        private readonly AutoResetEvent _autoResetEvent;
        private readonly InstanceHost _instanceHost;
        private readonly Mock<IPlugInFactory> _mockPlugInFactory;
        private readonly Mock<IRuntimeAssemblyLoadContext> _mockRuntimeAssemblyLoadContext;
        private readonly Mock<IRuntimeAssemblyLoadContextFactory> _mockRuntimeAssemblyLoadContextFactory;
        private InstanceState _waitingState;

        public InstanceHostTests(ITestOutputHelper output, TestFixture fixture) : base(output)
        {
            ArgumentNullException.ThrowIfNull(output);
            ArgumentNullException.ThrowIfNull(fixture);

            _mockRuntimeAssemblyLoadContextFactory = new Mock<IRuntimeAssemblyLoadContextFactory>();
            _mockRuntimeAssemblyLoadContext = new Mock<IRuntimeAssemblyLoadContext>();
            _mockPlugInFactory = new Mock<IPlugInFactory>();

            var optionsInstance = new RuntimeOptions()
            {
                MaxExecutingInstances = 5
            };

            _instanceHost = new InstanceHost(
                LoggerFactory!.CreateLogger<InstanceHost>(),
                _mockPlugInFactory.Object,
                Options.Create(optionsInstance),
                _mockRuntimeAssemblyLoadContextFactory.Object);

            _autoResetEvent = new AutoResetEvent(false);

            _instanceHost.InstanceStateChanged += InstanceHostInstanceStateChanged;
        }

        public void Dispose()
        {
            _instanceHost.InstanceStateChanged -= InstanceHostInstanceStateChanged;
        }

        [Fact]
        public void TestAddInstance()
        {
            SetupStateWait(InstanceState.None);

            var instance = _instanceHost.AddInstance(1, "name", "assembly");

            Assert.True(WaitForStateChange());

            Assert.NotNull(instance);
        }

        [Fact]
        public void TestAddInstanceExists()
        {
            _instanceHost._executingInstances.Add(new Instance()
            {
                Id = 1
            });

            Assert.Throws<InstanceExistsException>(() => _instanceHost.AddInstance(1, "name", "assembly"));
        }

        [Fact]
        public void TestAddInstanceInvalidAssembly()
        {
            Assert.Throws<ArgumentNullException>(() => _instanceHost.AddInstance(1, "name", null));
        }

        [Fact]
        public void TestAddInstanceInvalidId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _instanceHost.AddInstance(0, null, null));
        }

        [Fact]
        public void TestAddInstanceInvalidName()
        {
            Assert.Throws<ArgumentNullException>(() => _instanceHost.AddInstance(1, null, null));
        }

        [Fact]
        public void TestAddInstanceMaxRunning()
        {
            for (int i = 1; i < 6; i++)
            {
                _instanceHost._executingInstances.Add(new Instance()
                {
                    Id = i,
                    Name = i.ToString()
                });
            }

            Assert.Throws<RuntimeMaxInstancesRunningException>(() => _instanceHost.AddInstance(10, "name", "assembly"));
        }

        [Fact]
        public void TestRemoveInstanceRunning()
        {
            _instanceHost._executingInstances.Add(new Instance()
            {
                Id = 1,
                Name = "Test",
                State = InstanceState.Running
            });

            Assert.Throws<InstanceRunningException>(() => _instanceHost.RemoveInstance(1));
        }

        [Fact]
        public void TestRemoveInstanceInvalidId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _instanceHost.RemoveInstance(0));
        }

        [Fact]
        public void TestRemoveInstanceNotFound()
        {
            Assert.Throws<InstanceNotFoundException>(() => _instanceHost.RemoveInstance(1));
        }

        [Fact]
        public void TestStartInstanceComplete()
        {
            _instanceHost._executingInstances.Add(new Instance()
            {
                Id = 1,
            });

            SetupMockPlugIn();

            SetupStateWait(InstanceState.Complete);

            var instance = _instanceHost.StartInstance(1);

            Assert.True(WaitForStateChange());

            Assert.NotNull(instance);
            Assert.Equal(InstanceState.Complete, instance.State);
        }

        [Fact]
        public void TestStartInstanceFaulted()
        {
            _instanceHost._executingInstances.Add(new Instance()
            {
                Id = 1,
            });

            var mockPlugIn = SetupMockPlugIn();

            mockPlugIn
                .Setup(_ => _.ExecuteAsync(It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            SetupStateWait(InstanceState.Faulted);

            var instance = _instanceHost.StartInstance(1);

            Assert.True(WaitForStateChange());

            Assert.NotNull(instance);
            Assert.NotNull(instance.Exception);
            Assert.NotNull(instance.PlugIn);
            Assert.Equal(InstanceState.Faulted, instance.State);

            _mockPlugInFactory
                .Verify(_ => _.CreateInstance(
                    It.IsAny<Assembly>()),
                    Times.Once);

            mockPlugIn
                .Verify(_ => _.ExecuteAsync(
                    It.IsAny<CancellationToken>()),
                    Times.Once);

            OutputHelper.WriteLine(instance.ToString());
        }

        [Fact]
        public void TestStartInstanceNotFound()
        {
            Assert.Throws<InstanceNotFoundException>(() => _instanceHost.StartInstance(1));
        }

        [Fact]
        public void TestStartInstanceRunning()
        {
            _instanceHost._executingInstances.Add(new Instance()
            {
                Id = 2,
                Name = "Test",
                State = InstanceState.Running
            });

            var instance = _instanceHost.StartInstance(2);

            Assert.NotNull(instance);
            Assert.Equal(InstanceState.Running, instance.State);
        }

        [Fact]
        public void TestStopInstanceNotFound()
        {
            Assert.Throws<InstanceNotFoundException>(() => _instanceHost.StopInstance(1));
        }

        [Fact]
        public void TestStopInstanceNotRunning()
        {
            _instanceHost._executingInstances.Add(new Instance()
            {
                Id = 1,
                Name = "Test",
                State = InstanceState.PlugInLoadFailure
            });

            var instance = _instanceHost.StopInstance(1);

            Assert.NotNull(instance);
            Assert.Equal(InstanceState.PlugInLoadFailure, instance.State);
        }

        [Fact]
        public async Task TestStopInstanceRunning()
        {
            var tokenSource = new CancellationTokenSource();

            var instance = new Instance()
            {
                Id = 1,
                Name = "InstanceName",
                State = InstanceState.Running,
                TokenSource = tokenSource
            };

            instance.Task = Task.Run(async () => await WaitTaskAsync(tokenSource.Token))
                .ContinueWith((antecedent) => UpdateState(antecedent, instance));

            _instanceHost._executingInstances.Add(instance);

            _instanceHost.StopInstance(1);

            var executingInstance = await WaitForState(InstanceState.Complete);

            Assert.NotNull(executingInstance);
            Assert.Equal(InstanceState.Complete, executingInstance.State);
        }

        private void InstanceHostInstanceStateChanged(object? sender, InstanceStateEventArgs e)
        {
            if (e.State == _waitingState)
            {
                _autoResetEvent.Set();
            }
        }

        private Mock<IPlugIn> SetupMockPlugIn()
        {
            var mockPlugIn = new Mock<IPlugIn>();

            _mockRuntimeAssemblyLoadContextFactory
               .Setup(_ => _.Create(
                   It.IsAny<string>()))
               .Returns(_mockRuntimeAssemblyLoadContext.Object);

            _mockRuntimeAssemblyLoadContext
                .Setup(_ => _.LoadFromAssemblyPath(
                    It.IsAny<string>()))
                .Returns(typeof(object).Assembly);

            _mockPlugInFactory
                .Setup(_ => _.CreateInstance(
                    It.IsAny<Assembly>()))
                .Returns(mockPlugIn.Object);

            return mockPlugIn;
        }

        private void SetupStateWait(InstanceState state)
        {
            _autoResetEvent.Reset();

            _waitingState = state;
        }

        private void UpdateState(
            Task antecedent,
            Instance executingInstance)
        {
            OutputHelper.WriteLine("Waiting Task complete");
            executingInstance.State = InstanceState.Complete;
        }

        private async Task<Instance?> WaitForState(InstanceState state)
        {
            int i = 0;
            Instance? executingInstance;

            do
            {
                executingInstance = _instanceHost._executingInstances[0];

                await Task.Delay(WaitDelay);

                i++;
            } while (executingInstance != null && executingInstance.State != state && i <= WaitItterations);

            return executingInstance;
        }

        private bool WaitForStateChange()
        {
            return _autoResetEvent.WaitOne(TimeSpan.FromMilliseconds(1000));
        }

        private async Task WaitTaskAsync(
            CancellationToken cancellationToken)
        {
            do
            {
                OutputHelper.WriteLine("Executing Waiting Task");

                await Task.Delay(100, cancellationToken);
            }
            while (cancellationToken.IsCancellationRequested);
        }
    }
}