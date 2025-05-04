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
using Shaos.Services.Runtime;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Factories;
using Shaos.Services.Runtime.Host;
using Shaos.Services.UnitTests.Fixtures;
using Shaos.Testing.Shared;
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
        private readonly Mock<IRuntimeAssemblyLoadContext> _mockRuntimeAssemblyLoadContext;
        private readonly Mock<IRuntimeAssemblyLoadContextFactory> _mockRuntimeAssemblyLoadContextFactory;
        private readonly Mock<IPlugInFactory> _mockPlugInFactory;
        private InstanceState _waitingState;

        public InstanceHostTests(ITestOutputHelper output, TestFixture fixture) : base(output)
        {
            ArgumentNullException.ThrowIfNull(output);
            ArgumentNullException.ThrowIfNull(fixture);

            _mockRuntimeAssemblyLoadContextFactory = new Mock<IRuntimeAssemblyLoadContextFactory>();
            _mockRuntimeAssemblyLoadContext = new Mock<IRuntimeAssemblyLoadContext>();
            _mockPlugInFactory = new Mock<IPlugInFactory>();

            var optionsInstance = new InstanceHostOptions()
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
        public void TestAddInstanceInvalidAssembly()
        {
            Assert.Throws<ArgumentNullException>(() => _instanceHost.CreateInstance(1, "name", null!));
        }

        [Fact]
        public void TestAddInstanceInvalidId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _instanceHost.CreateInstance(0, null!, null!));
        }

        [Fact]
        public void TestAddInstanceInvalidName()
        {
            Assert.Throws<ArgumentNullException>(() => _instanceHost.CreateInstance(1, null!, null!));
        }

        [Fact]
        public void TestAddInstanceMaxRunning()
        {
            for (int i = 1; i < 6; i++)
            {
                _instanceHost
                    ._executingInstances
                    .Add(new Instance(i, i.ToString(), "AssemblyPath"));
            }

            Assert.Throws<MaxInstancesRunningException>(() => _instanceHost.CreateInstance(10, "name", "assembly"));
        }

        [Fact]
        public void TestCreateInstanceException()
        {
            SetupAssemblyLoadContext();

            SetupStateWait(InstanceState.None);

            var instance = _instanceHost.CreateInstance(1, "name", "assembly");

            Assert.True(WaitForStateChange());

            Assert.NotNull(instance);
            Assert.Equal(InstanceState.None, instance.State);
        }

        [Fact]
        public void TestCreateInstanceExists()
        {
            _instanceHost._executingInstances.Add(new Instance(1, "Test", "AssemblyPath"));

            Assert.Throws<InstanceExistsException>(() => _instanceHost.CreateInstance(1, "name", "assembly"));
        }

        [Fact]
        public void TestInstanceExistsFalse()
        {
            Assert.False(_instanceHost.InstanceExists(1));
        }

        [Fact]
        public void TestInstanceExistsTrue()
        {
            _instanceHost._executingInstances.Add(
                new Instance(1, "Test", "AssemblyPath"));

            Assert.True(_instanceHost.InstanceExists(1));
        }

        [Fact]
        public void TestRemoveInstance()
        {
            _instanceHost._executingInstances.Add(
                new Instance(1, "Test", "AssemblyPath", InstanceState.Complete));

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
            _instanceHost._executingInstances.Add(
                new Instance(1, "Test", "AssemblyPath", InstanceState.Running));

            Assert.Throws<InstanceRunningException>(() => _instanceHost.RemoveInstance(1));
        }

        //        [Fact]
        //        public void TestStartInstanceAwaitingIn()
        //        {
        //            var mockPlugIn = new Mock<IPlugIn>();

        //            _instanceHost._executingInstances.Add(
        //                new Instance(1, "Test", "AssemblyPath", InstanceState.PlugInLoaded));

        //            mockPlugIn
        //                .Setup(_ => _.ExecuteAsync(It.IsAny<CancellationToken>()))
        //                .Throws<Exception>();

        //            SetupStateWait(InstanceState.Faulted);

        //            var instance = _instanceHost.StartInstance(1);

        //            Assert.True(WaitForStateChange());

        //#warning TODO
        //            //Assert.NotNull(instance);
        //            //Assert.NotNull(instance.Exception);
        //            //Assert.NotNull(instance.PlugIn);
        //            //Assert.Equal(InstanceState.Faulted, instance.State);

        //            mockPlugIn
        //                .Verify(_ => _.ExecuteAsync(
        //                    It.IsAny<CancellationToken>()),
        //                    Times.Once);

        //            OutputHelper.WriteLine(instance.ToString());
        //        }

        //        [Fact]
        //        public void TestStartInstanceComplete()
        //        {
        //            var mockPlugIn = new Mock<IPlugIn>();

        //            _instanceHost._executingInstances.Add(
        //                new Instance(1, "Test", "AssemblyPath", InstanceState.PlugInLoaded));

        //#warning TODO
        //            //_instanceHost._executingInstances.Add(new Instance()
        //            //{
        //            //    Id = 1,
        //            //    PlugIn = mockPlugIn.Object,
        //            //    State = InstanceState.PlugInLoaded
        //            //});

        //            SetupStateWait(InstanceState.Complete);

        //            var instance = _instanceHost.StartInstance(1);

        //            Assert.True(WaitForStateChange());

        //            Assert.NotNull(instance);
        //            Assert.Equal(InstanceState.Complete, instance.State);
        //        }

        [Fact]
        public void TestStartInstanceNotFound()
        {
            Assert.Throws<InstanceNotFoundException>(() => _instanceHost.StartInstance(1));
        }

        [Fact]
        public void TestStartInstanceRunning()
        {
            _instanceHost._executingInstances.Add(
                new Instance(2, "Test", "AssemblyPath", InstanceState.Running));

            var instance = _instanceHost.StartInstance(2);

            Assert.NotNull(instance);
            Assert.Equal(InstanceState.Running, instance.State);
        }

        [Fact]
        public void TestStopInstanceNotFound()
        {
            Assert.Throws<InstanceNotFoundException>(() => _instanceHost.StopInstance(1));
        }

        //[Fact]
        //public void TestStopInstanceNotRunning()
        //{
        //    _instanceHost._executingInstances.Add(
        //        new Instance(1, "Test", "AssemblyPath", InstanceState.PlugInLoadFailure));

        //    var instance = _instanceHost.StopInstance(1);

        //    Assert.NotNull(instance);
        //    Assert.Equal(InstanceState.PlugInLoadFailure, instance.State);
        //}

        [Fact]
        public async Task TestStopInstanceRunning()
        {
            var tokenSource = new CancellationTokenSource();

            _instanceHost._executingInstances.Add(
                new Instance(1, "Test", "AssemblyPath", InstanceState.Running));

#warning TODO
            //instance.Task = Task.Run(async () => await WaitTaskAsync(tokenSource.Token))
            //    .ContinueWith((antecedent) => UpdateState(antecedent, instance));

            //_instanceHost._executingInstances.Add(instance);

            //_instanceHost.StopInstance(1);

            //var executingInstance = await WaitForState(InstanceState.Complete);

            //Assert.NotNull(executingInstance);
            //Assert.Equal(InstanceState.Complete, executingInstance.State);
        }

        private void InstanceHostInstanceStateChanged(object? sender, InstanceStateEventArgs e)
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

        private void SetupStateWait(InstanceState state)
        {
            _autoResetEvent.Reset();

            _waitingState = state;
        }

        private void UpdateState(
            Task antecedent,
            Instance executingInstance)
        {
#warning TODO
            OutputHelper.WriteLine("Waiting Task complete");
            //executingInstance.State = InstanceState.Complete;
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