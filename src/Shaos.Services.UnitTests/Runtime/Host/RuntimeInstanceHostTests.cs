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
using Shaos.Services.Runtime.Host;
using Shaos.Services.UnitTests.Fixtures;
using Shaos.Testing.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.Runtime.Host
{
    public class RuntimeInstanceHostTests : BaseTests, IClassFixture<TestFixture>, IDisposable
    {
        private const string AssemblyPath = "AssemblyPath";
        private const string InstanceName = "Test";
        private const int WaitDelay = 10;
        private const int WaitItterations = 500;
        private readonly AutoResetEvent _autoResetEvent;
        private readonly RuntimeInstanceHost _instanceHost;
        private readonly Mock<IPlugIn> _mockPlugIn;
        private readonly Mock<IRuntimeAssemblyLoadContext> _mockRuntimeAssemblyLoadContext;
        private readonly Mock<IRuntimeAssemblyLoadContextFactory> _mockRuntimeAssemblyLoadContextFactory;
        private RuntimeInstanceState _waitingState;

        public RuntimeInstanceHostTests(ITestOutputHelper output, TestFixture fixture) : base(output)
        {
            ArgumentNullException.ThrowIfNull(output);
            ArgumentNullException.ThrowIfNull(fixture);

            _mockPlugIn = new Mock<IPlugIn>();
            _mockRuntimeAssemblyLoadContext = new Mock<IRuntimeAssemblyLoadContext>();
            _mockRuntimeAssemblyLoadContextFactory = new Mock<IRuntimeAssemblyLoadContextFactory>();

            var optionsInstance = new RuntimeInstanceHostOptions()
            {
                MaxExecutingInstances = 5
            };

            _instanceHost = new RuntimeInstanceHost(LoggerFactory!.CreateLogger<RuntimeInstanceHost>(),
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
        public void TestCreateInstanceExists()
        {
            SetupExecutingInstance();

            Assert.Throws<InstanceExistsException>(() => _instanceHost.CreateInstance(1,
                                                                                      1,
                                                                                      InstanceName,
                                                                                      AssemblyPath));
        }

        [Fact]
        public void TestCreateInstanceInvalidAssembly()
        {
            Assert.Throws<ArgumentNullException>(() => _instanceHost.CreateInstance(1,
                                                                                    2,
                                                                                    InstanceName,
                                                                                    null!));
        }

        [Fact]
        public void TestCreateInstanceInvalidId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _instanceHost.CreateInstance(0,
                                                                                          1,
                                                                                          null!,
                                                                                          null!));
        }

        [Fact]
        public void TestCreateInstanceInvalidName()
        {
            Assert.Throws<ArgumentNullException>(() => _instanceHost.CreateInstance(1,
                                                                                    2,
                                                                                    null!,
                                                                                    null!,
                                                                                    true));
        }

        [Fact]
        public void TestCreateInstanceMaxRunning()
        {
            for (int i = 1; i < 6; i++)
            {
                _instanceHost
                    ._executingInstances
                    .Add(new RuntimeInstance(i, i, i.ToString(), AssemblyPath, false));
            }

            Assert.Throws<MaxInstancesRunningException>(() => _instanceHost.CreateInstance(10,
                                                                                           1,
                                                                                           InstanceName,
                                                                                           AssemblyPath,
                                                                                           true));
        }

        [Fact]
        public void TestCreateInstanceSuccess()
        {
            SetupAssemblyLoadContext();

            SetupStateWait(RuntimeInstanceState.None);

            var instance = _instanceHost.CreateInstance(1, 1, InstanceName, AssemblyPath);

            Assert.True(WaitForStateChange());

            Assert.NotNull(instance);
            Assert.Equal(RuntimeInstanceState.None, instance.State);
        }

        [Fact]
        public void TestInstanceExistsFalse()
        {
            Assert.False(_instanceHost.InstanceExists(1));
        }

        [Fact]
        public void TestInstanceExistsTrue()
        {
            SetupExecutingInstance();

            Assert.True(_instanceHost.InstanceExists(1));
        }

        [Fact]
        public void TestRemoveInstance()
        {
            SetupExecutingInstance();

            _instanceHost.RemoveInstance(1);

            Assert.Empty(_instanceHost._executingInstances);
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
        public void TestRemoveInstanceRunning()
        {
            SetupExecutingInstance(RuntimeInstanceState.Running);

            Assert.Throws<InstanceRunningException>(() => _instanceHost.RemoveInstance(1));
        }

        [Fact]
        public void TestStartInstanceNotFound()
        {
            Assert.Throws<InstanceNotFoundException>(() => _instanceHost.StartInstance(1, _mockPlugIn.Object));
        }

        [Fact]
        public void TestStartInstancePlugInLoaded()
        {
            _instanceHost._executingInstances.Add(
                new RuntimeInstance(1, 1, InstanceName, AssemblyPath, RuntimeInstanceState.None));

            var instanceLoadContext = new RuntimeInstanceLoadContext(_mockRuntimeAssemblyLoadContext.Object);

            _instanceHost._instanceLoadContexts.Add(1, instanceLoadContext);

            SetupStateWait(RuntimeInstanceState.Running);

            var instance = _instanceHost.StartInstance(1, _mockPlugIn.Object);

            Assert.True(WaitForStateChange());

            Assert.NotNull(instance);
            Assert.NotNull(instance.ExecutionContext);
            Assert.Equal(RuntimeInstanceState.Running, instance.State);
        }

        [Fact]
        public void TestStartInstanceRunning()
        {
            SetupExecutingInstance(RuntimeInstanceState.Running);

            Assert.Throws<InstanceRunningException>(() => _instanceHost.StartInstance(1, _mockPlugIn.Object));
        }

        [Fact]
        public void TestStopInstanceInvalidId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _instanceHost.StopInstance(0));
        }

        [Fact]
        public void TestStopInstanceNotFound()
        {
            Assert.Throws<InstanceNotFoundException>(() => _instanceHost.StopInstance(1));
        }

        [Fact]
        public async Task TestStopInstanceRunning()
        {
            var instance = new RuntimeInstance(1, 1, InstanceName, AssemblyPath, RuntimeInstanceState.Running);

            instance.LoadContext(_mockPlugIn.Object);

            instance.StartExecution(
                (cancellationToken) => WaitTaskAsync(cancellationToken),
                (task) => _instanceHost.UpdateStateOnCompletion(instance, task));

            _instanceHost._executingInstances.Add(instance);

            _instanceHost.StopInstance(1);

            var executingInstance = await WaitForState(RuntimeInstanceState.Complete);

            Assert.NotNull(executingInstance);
            Assert.Equal(RuntimeInstanceState.Complete, executingInstance.State);
        }

        private void InstanceHostInstanceStateChanged(object? sender, RuntimeInstanceStateEventArgs e)
        {
            if (e.State == _waitingState)
            {
                _autoResetEvent.Set();
            }
        }

        private void SetupAssemblyLoadContext()
        {
            _mockRuntimeAssemblyLoadContextFactory
               .Setup(_ => _.Create(
                   It.IsAny<string>()))
               .Returns(_mockRuntimeAssemblyLoadContext.Object);

            _mockRuntimeAssemblyLoadContext
                .Setup(_ => _.LoadFromAssemblyPath(
                    It.IsAny<string>()))
                .Returns(typeof(object).Assembly);
        }

        private void SetupExecutingInstance(RuntimeInstanceState state = RuntimeInstanceState.None)
        {
            _instanceHost._executingInstances.Add(
                new RuntimeInstance(1, 1, InstanceName, AssemblyPath, state));
        }

        private void SetupStateWait(RuntimeInstanceState state)
        {
            _autoResetEvent.Reset();

            _waitingState = state;
        }

        private async Task<RuntimeInstance?> WaitForState(RuntimeInstanceState state)
        {
            int i = 0;
            RuntimeInstance? executingInstance;

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

        private async Task WaitTaskAsync(CancellationToken cancellationToken)
        {
            do
            {
                OutputHelper.WriteLine("Executing Waiting Task");

                await Task.Delay(100, cancellationToken);
            }
            while (cancellationToken.IsCancellationRequested);
        }

        public class Test
        {
            public int Value { get; set; }
        }
    }
}