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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shaos.Sdk;
using Shaos.Services.Runtime;
using Shaos.Services.Runtime.Exceptions;
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
        private const string AssemblyPath = "AssemblyPath";
        private const string InstanceName = "Test";
        private const int WaitDelay = 10;
        private const int WaitItterations = 500;
        private readonly AutoResetEvent _autoResetEvent;
        private readonly InstanceHost _instanceHost;
        private readonly Mock<IPlugIn> _mockPlugIn;
        private readonly Mock<IPlugInBuilder> _mockPlugInBuilder;
        private readonly Mock<IPlugInConfigurationBuilder> _mockPlugInConfigurationBuilder;
        private readonly Mock<IRuntimeAssemblyLoadContext> _mockRuntimeAssemblyLoadContext;
        private readonly Mock<IRuntimeAssemblyLoadContextFactory> _mockRuntimeAssemblyLoadContextFactory;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IServiceScope> _mockServiceScope;
        private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private InstanceState _waitingState;

        public InstanceHostTests(ITestOutputHelper output, TestFixture fixture) : base(output)
        {
            ArgumentNullException.ThrowIfNull(output);
            ArgumentNullException.ThrowIfNull(fixture);

            _mockPlugIn = new Mock<IPlugIn>();
            _mockPlugInBuilder = new Mock<IPlugInBuilder>();
            _mockPlugInConfigurationBuilder = new Mock<IPlugInConfigurationBuilder>();
            _mockRuntimeAssemblyLoadContext = new Mock<IRuntimeAssemblyLoadContext>();
            _mockRuntimeAssemblyLoadContextFactory = new Mock<IRuntimeAssemblyLoadContextFactory>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScope = new Mock<IServiceScope>();

            var optionsInstance = new InstanceHostOptions()
            {
                MaxExecutingInstances = 5
            };

            _instanceHost = new InstanceHost(LoggerFactory!.CreateLogger<InstanceHost>(),
                                             Options.Create(optionsInstance),
                                             _mockServiceScopeFactory.Object,
                                             _mockPlugInConfigurationBuilder.Object,
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

            var configuration = new InstanceConfiguration(true, string.Empty);

            Assert.Throws<InstanceExistsException>(() => _instanceHost.CreateInstance(1,
                                                                                      1,
                                                                                      InstanceName,
                                                                                      AssemblyPath,
                                                                                      configuration));
        }

        [Fact]
        public void TestCreateInstanceInvalidAssembly()
        {
            Assert.Throws<ArgumentNullException>(() => _instanceHost.CreateInstance(1,
                                                                                    2,
                                                                                    InstanceName,
                                                                                    null!,
                                                                                    null!));
        }

        [Fact]
        public void TestCreateInstanceInvalidId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _instanceHost.CreateInstance(0,
                                                                                          1,
                                                                                          null!,
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
                                                                                    null!));
        }

        [Fact]
        public void TestCreateInstanceMaxRunning()
        {
            var configuration = new InstanceConfiguration(true, string.Empty);

            for (int i = 1; i < 6; i++)
            {
                _instanceHost
                    ._executingInstances
                    .Add(new Instance(i, i, i.ToString(), AssemblyPath, configuration));
            }

            Assert.Throws<MaxInstancesRunningException>(() => _instanceHost.CreateInstance(10,
                                                                                           1,
                                                                                           InstanceName,
                                                                                           AssemblyPath,
                                                                                           configuration));
        }

        [Fact]
        public void TestCreateInstanceSuccess()
        {
            SetupAssemblyLoadContext();

            SetupStateWait(InstanceState.None);

            var configuration = new InstanceConfiguration(true, string.Empty);
            var instance = _instanceHost.CreateInstance(1, 1, InstanceName, AssemblyPath, configuration);

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
            SetupExecutingInstance();

            Assert.True(_instanceHost.InstanceExists(1));
        }

        [Fact]
        public void TestLoadConfiguration()
        {
            SetupExecutingInstance();

            _instanceHost
                ._instanceLoadContexts
                .Add(1, new InstanceLoadContext(_mockRuntimeAssemblyLoadContext.Object));

            _mockPlugInConfigurationBuilder
                .Setup(_ => _.LoadConfiguration(It.IsAny<Assembly>(), It.IsAny<string?>()))
                .Returns(new Test());

            var result = _instanceHost.LoadConfiguration(1);

            Assert.NotNull(result);
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
            SetupExecutingInstance(InstanceState.Running);

            Assert.Throws<InstanceRunningException>(() => _instanceHost.RemoveInstance(1));
        }

        [Fact]
        public void TestStartInstanceNotConfiguredRunning()
        {
            var configuration = new InstanceConfiguration(true, string.Empty);

            _instanceHost
                ._executingInstances
                .Add(new Instance(1, 1, InstanceName, AssemblyPath, InstanceState.None, configuration));

            Assert.Throws<InstanceNotConfiguredException>(() => _instanceHost.StartInstance(1));
        }

        [Fact]
        public void TestStartInstanceNotFound()
        {
            Assert.Throws<InstanceNotFoundException>(() => _instanceHost.StartInstance(1));
        }

        [Fact]
        public void TestStartInstancePlugInLoaded()
        {
            var configuration = new InstanceConfiguration(true, "config");

            _instanceHost._executingInstances.Add(
                new Instance(1, 1, InstanceName, AssemblyPath, InstanceState.None, configuration));

            var instanceLoadContext = new InstanceLoadContext(_mockRuntimeAssemblyLoadContext.Object);

            _instanceHost._instanceLoadContexts.Add(1, instanceLoadContext);

            SetupServiceScopeFactory();

            _mockServiceProvider
                .Setup(_ => _.GetService(It.IsAny<Type>()))
                .Returns(_mockPlugInBuilder.Object);

            _mockPlugInBuilder
                .Setup(_ => _.PlugIn)
                .Returns(_mockPlugIn.Object);

            SetupStateWait(InstanceState.Running);

            var instance = _instanceHost.StartInstance(1);

            Assert.True(WaitForStateChange());

            Assert.NotNull(instance);
            Assert.NotNull(instance.Context);
            Assert.Equal(InstanceState.Running, instance.State);
        }

        [Fact]
        public void TestStartInstancePlugInNotLoaded()
        {
            var configuration = new InstanceConfiguration(true, "config");

            _instanceHost._executingInstances.Add(
                new Instance(1, 1, InstanceName, AssemblyPath, InstanceState.None, configuration));

            var instanceLoadContext = new InstanceLoadContext(_mockRuntimeAssemblyLoadContext.Object);

            _instanceHost._instanceLoadContexts.Add(1, instanceLoadContext);

            SetupServiceScopeFactory();

            _mockServiceProvider
                .Setup(_ => _.GetService(It.IsAny<Type>()))
                .Returns(_mockPlugInBuilder.Object);

            Assert.Throws<PlugInInstanceTypeNotCreatedException>(() => _instanceHost.StartInstance(1));
        }

        [Fact]
        public void TestStartInstanceRunning()
        {
            SetupExecutingInstance(InstanceState.Running);

            Assert.Throws<InstanceRunningException>(() => _instanceHost.StartInstance(1));
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
            var configuration = new InstanceConfiguration(true, "config");

            var instance = new Instance(1, 1, InstanceName, AssemblyPath, InstanceState.Running, configuration);

            instance.LoadContext(_mockPlugIn.Object);

            instance.StartExecution(
                (cancellationToken) => WaitTaskAsync(cancellationToken),
                (task) => _instanceHost.UpdateStateOnCompletion(instance, task));

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

        private void SetupExecutingInstance(InstanceState state = InstanceState.None)
        {
            var configuration = new InstanceConfiguration(true, string.Empty);

            _instanceHost._executingInstances.Add(
                new Instance(1, 1, InstanceName, AssemblyPath, state, configuration));
        }

        private void SetupServiceScopeFactory()
        {
            _mockServiceScopeFactory
                .Setup(_ => _.CreateScope())
                .Returns(_mockServiceScope.Object);

            _mockServiceScope
                .Setup(_ => _.ServiceProvider)
                .Returns(_mockServiceProvider.Object);
        }

        private void SetupStateWait(InstanceState state)
        {
            _autoResetEvent.Reset();

            _waitingState = state;
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

        public class Test
        {
            public int Value { get; set; }
        }
    }
}