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
using Shaos.Services.Runtime.Factories;
using Shaos.Services.Runtime.Host;
using Shaos.Services.UnitTests.Fixtures;
using Shaos.Testing.Shared;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.Runtime.Host
{
    public class InstanceHostTests : BaseTests, IClassFixture<TestFixture>, IDisposable
    {
        private const int WaitDelay = 10;
        private const int WaitItterations = 500;

        private readonly AutoResetEvent _autoResetEvent;
        private readonly InstanceHost _instanceHost;
        private readonly Mock<IPlugIn> _mockPlugIn;
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
            _mockPlugIn = new Mock<IPlugIn>();

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
        public void TestCreateInstanceExists()
        {
            _instanceHost
                ._executingInstances
                .Add(new Instance(1, "Test", "AssemblyPath", false));

            Assert.Throws<InstanceExistsException>(() => _instanceHost.CreateInstance(1, "name", "assembly", false));
        }

        [Fact]
        public void TestCreateInstanceInvalidAssemblyPath()
        {
            Assert.Throws<ArgumentNullException>(() => _instanceHost.CreateInstance(1, "name", null!, false));
        }

        [Fact]
        public void TestCreateInstanceInvalidId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _instanceHost.CreateInstance(0, null!, null!, false));
        }

        [Fact]
        public void TestCreateInstanceInvalidName()
        {
            Assert.Throws<ArgumentNullException>(() => _instanceHost.CreateInstance(1, null!, null!, false));
        }

        [Fact]
        public void TestCreateInstanceMaxRunning()
        {
            for (int i = 1; i < 6; i++)
            {
                _instanceHost
                    ._executingInstances
                    .Add(new Instance(i, i.ToString(), "AssemblyPath", false));
            }

            Assert.Throws<MaxInstancesRunningException>(() => _instanceHost.CreateInstance(10, "name", "assembly", false));
        }

        [Fact]
        public void TestCreateInstanceSuccess()
        {
            SetupAssemblyLoadContext();

            SetupStateWait(InstanceState.None);

            var instance = _instanceHost.CreateInstance(1, "name", "assembly", false);

            Assert.True(WaitForStateChange());

            Assert.NotNull(instance);
            Assert.Equal(InstanceState.None, instance.State);
        }

        [Fact]
        public void TestInstanceExistsFalse()
        {
            Assert.False(_instanceHost.InstanceExists(1));
        }

        [Fact]
        public void TestInstanceExistsTrue()
        {
            _instanceHost
                ._executingInstances
                .Add(new Instance(1, "Test", "AssemblyPath", false));

            Assert.True(_instanceHost.InstanceExists(1));
        }

        [Fact]
        public void TestLoadInstance()
        {
            var plugIn = _mockPlugIn.Object;
            object config = new object();

            _mockRuntimeAssemblyLoadContextFactory
                .Setup(_ => _.Create(It.IsAny<string>()))
                .Returns(_mockRuntimeAssemblyLoadContext.Object);

            _mockPlugInFactory
                .Setup(_ => _.CreateInstance(It.IsAny<Assembly>(), out plugIn, out config));

            _instanceHost
                ._executingInstances
                .Add(new Instance(1, "Test", "AssemblyPath", InstanceState.None));

            SetupStateWait(InstanceState.Loaded);

            var instance = _instanceHost.LoadInstance(1);

            Assert.True(WaitForStateChange());

            Assert.NotNull(instance);
            Assert.Equal(InstanceState.Loaded, instance.State);
        }

        [Fact]
        public void TestLoadInstanceFaulted()
        {
            _mockRuntimeAssemblyLoadContextFactory
                .Setup(_ => _.Create(It.IsAny<string>()))
                .Throws(new Exception());

            _instanceHost._executingInstances.Add(new Instance(1, "Test", "AssemblyPath", InstanceState.None));

            SetupStateWait(InstanceState.Faulted);

            var instance = _instanceHost.LoadInstance(1);

            Assert.True(WaitForStateChange());

            Assert.NotNull(instance);
            Assert.NotNull(instance.Exception);
            Assert.Equal(InstanceState.Faulted, instance.State);
        }

        [Fact]
        public void TestLoadInstanceLoaded()
        {
            _instanceHost._executingInstances.Add(new Instance(1, "Test", "AssemblyPath", InstanceState.Loaded));

            Assert.Throws<InstanceLoadedException>(() => _instanceHost.LoadInstance(1));
        }

        [Fact]
        public void TestLoadInstanceNotFound()
        {
            Assert.Throws<InstanceNotFoundException>(() => _instanceHost.LoadInstance(1));
        }

        [Fact]
        public void TestRemoveInstance()
        {
            _instanceHost._executingInstances.Add(
                new Instance(1, "Test", "AssemblyPath", InstanceState.None));

            _instanceHost.RemoveInstance(1);

            Assert.Empty(_instanceHost._executingInstances);
        }

        [Fact]
        public void TestRemoveInstanceInvalidId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _instanceHost.RemoveInstance(0));
        }

        [Theory]
        [InlineData(InstanceState.Complete)]
        [InlineData(InstanceState.Faulted)]
        [InlineData(InstanceState.Loaded)]
        [InlineData(InstanceState.Running)]
        [InlineData(InstanceState.Starting)]
        public void TestRemoveInstanceInvalidState(InstanceState state)
        {
            _instanceHost._executingInstances.Add(
                new Instance(1, "Test", "AssemblyPath", state));

            Assert.Throws<InstanceInvalidStateException>(() => _instanceHost.RemoveInstance(1));
        }

        [Fact]
        public void TestRemoveInstanceNotFound()
        {
            Assert.Throws<InstanceNotFoundException>(() => _instanceHost.RemoveInstance(1));
        }

        [Fact]
        public void TestStartInstance()
        {
            _instanceHost._executingInstances.Add(
                new Instance(1, "Test", "AssemblyPath", InstanceState.Loaded));

            SetupStateWait(InstanceState.Starting);

            var instance = _instanceHost.StartInstance(1);

            Assert.True(WaitForStateChange());

            Assert.NotNull(instance);
            Assert.Equal(InstanceState.Starting, instance.State);

            OutputHelper.WriteLine(instance.ToString());
        }

        [Theory]
        [InlineData(InstanceState.None)]
        [InlineData(InstanceState.Running)]
        public void TestStartInstanceInvalidState(InstanceState state)
        {
            _instanceHost._executingInstances.Add(
                new Instance(1, "Test", "AssemblyPath", state));

            Assert.Throws<InstanceInvalidStateException>(() => _instanceHost.StartInstance(1));
        }

        [Fact]
        public void TestStartInstanceNotFound()
        {
            Assert.Throws<InstanceNotFoundException>(() => _instanceHost.StartInstance(1));
        }

        [Theory]
        [InlineData(InstanceState.Complete)]
        [InlineData(InstanceState.Faulted)]
        [InlineData(InstanceState.Loaded)]
        [InlineData(InstanceState.None)]
        [InlineData(InstanceState.Starting)]
        public void TestStopInstanceInvalidState(InstanceState state)
        {
            _instanceHost._executingInstances.Add(
                new Instance(1, "Test", "AssemblyPath", state));

            Assert.Throws<InstanceInvalidStateException>(() => _instanceHost.StopInstance(1));
        }

        [Fact]
        public void TestStopInstanceNotFound()
        {
            Assert.Throws<InstanceNotFoundException>(() => _instanceHost.StopInstance(1));
        }

        [Fact]
        public async Task TestStopInstanceRunning()
        {
            using var cancellationTokenSource = new CancellationTokenSource();

            var instance = new Instance(1, "Test", "AssemblyPath", InstanceState.Running);

            instance.SetupTask(
                Task.Run(async () => await WaitTaskAsync(cancellationTokenSource.Token))
                .ContinueWith((antecedent) => UpdateState(antecedent, instance)),
                cancellationTokenSource);

            _instanceHost._executingInstances.Add(instance);

            _instanceHost.StopInstance(1);

            var executingInstance = await WaitForState(InstanceState.Complete);

            Assert.NotNull(executingInstance);
            Assert.Equal(InstanceState.Complete, executingInstance.State);
        }

        [Theory]
        [InlineData(InstanceState.Complete)]
        [InlineData(InstanceState.Faulted)]
        [InlineData(InstanceState.Loaded)]
        public void TestUnloadInstance(InstanceState state)
        {
            _instanceHost._executingInstances.Add(
                new Instance(1, "Test", "AssemblyPath", state));

            SetupStateWait(InstanceState.None);

            var instance = _instanceHost.UnloadInstance(1);

            Assert.True(WaitForStateChange());
        }

        [Theory]
        [InlineData(InstanceState.None)]
        [InlineData(InstanceState.Starting)]
        public void TestUnloadInstanceInvalidState(InstanceState state)
        {
            _instanceHost._executingInstances.Add(
                new Instance(1, "Test", "AssemblyPath", state));

            Assert.Throws<InstanceInvalidStateException>(() => _instanceHost.UnloadInstance(1));
        }

        [Fact]
        public void TestUnloadInstanceNotFound()
        {
            Assert.Throws<InstanceNotFoundException>(() => _instanceHost.UnloadInstance(1));
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
            OutputHelper.WriteLine("Waiting Task complete");
            executingInstance.SetComplete();
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