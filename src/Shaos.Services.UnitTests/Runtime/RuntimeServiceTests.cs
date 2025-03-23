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
using Shaos.Repository.Models;
using Shaos.Sdk;
using Shaos.Services.Exceptions;
using Shaos.Services.IO;
using Shaos.Services.Runtime;
using Shaos.Services.Shared.Tests;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.Runtime
{
    public class RuntimeServiceTests : BaseTests, IClassFixture<TestFixture>
    {
        private const int WaitDelay = 10;
        private const int WaitItterations = 500;

        private readonly TestFixture _fixture;
        private readonly Mock<IFileStoreService> _mockFileStoreService;
        private readonly Mock<IPlugInFactory> _mockPlugInFactory;
        private readonly Mock<IRuntimeAssemblyLoadContext> _mockRuntimeAssemblyLoadContext;
        private readonly Mock<IRuntimeAssemblyLoadContextFactory> _mockRuntimeAssemblyLoadContextFactory;
        private readonly RuntimeService _runtimeService;

        public RuntimeServiceTests(ITestOutputHelper output, TestFixture fixture) : base(output)
        {
            _fixture = fixture;

            _mockRuntimeAssemblyLoadContextFactory = new Mock<IRuntimeAssemblyLoadContextFactory>();
            _mockRuntimeAssemblyLoadContext = new Mock<IRuntimeAssemblyLoadContext>();
            _mockFileStoreService = new Mock<IFileStoreService>();
            _mockPlugInFactory = new Mock<IPlugInFactory>();

            var optionsInstance = new RuntimeServiceOptions()
            {
                MaxExecutingInstances = 5
            };

            var options = Options.Create(optionsInstance);

            _runtimeService = new RuntimeService(
                LoggerFactory!.CreateLogger<RuntimeService>(),
                options,
                _mockPlugInFactory.Object,
                _mockFileStoreService.Object,
                _mockRuntimeAssemblyLoadContextFactory.Object);
        }

        [Fact]
        public void TestGetExecutingInstanceFound()
        {
            _runtimeService._executingInstances.Add(new ExecutingInstance()
            {
                Id = 1,
                Name = "Test",
            });

            var executingInstance = _runtimeService.GetExecutingInstance(1);

            Assert.NotNull(executingInstance);
        }

        [Fact]
        public void TestGetExecutingInstanceNotFound()
        {
            var executingInstance = _runtimeService.GetExecutingInstance(1);

            Assert.Null(executingInstance);
        }

        [Fact]
        public void TestGetInstances()
        {
            for (int i = 1; i < 4; i++)
            {
                _runtimeService._executingInstances.Add(new ExecutingInstance()
                {
                    Id = i,
                    Name = "Test"
                });
            }

            var result = _runtimeService.GetExecutingInstances();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task TestStartInstanceAsync()
        {
            _mockFileStoreService
                .Setup(_ => _.GetAssemblyPath(It.IsAny<int>()))
                .Returns(_fixture.AssemblyDirectory!);

            var mockPlugIn = new Mock<IPlugIn>();

            _mockRuntimeAssemblyLoadContextFactory
                .Setup(_ => _.Create(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(_mockRuntimeAssemblyLoadContext.Object);

            _mockRuntimeAssemblyLoadContext
                .Setup(_ => _.LoadFromAssemblyName(
                    It.IsAny<AssemblyName>()))
                .Returns(typeof(object).Assembly);

            _mockPlugInFactory
                .Setup(_ => _.CreateInstance(
                    It.IsAny<Assembly>(),
                    It.IsAny<IRuntimeAssemblyLoadContext>()))
                .Returns(mockPlugIn.Object);

            SetupPlugInTypes(out PlugIn plugIn, out PlugInInstance plugInInstance);

            var result = _runtimeService
                .StartInstance(plugIn, plugInInstance);

            Assert.NotNull(result);
            Assert.Equal(ExecutionState.None, result.State);

            var instance = await WaitForState(2, ExecutionState.Complete);

            Assert.NotNull(instance);
            Assert.NotNull(instance.PlugIn);
            Assert.Equal(ExecutionState.Complete, instance.State);

            _mockPlugInFactory
                .Verify(_ => _.CreateInstance(
                    It.IsAny<Assembly>(),
                    It.IsAny<IRuntimeAssemblyLoadContext>()),
                    Times.Once);

            mockPlugIn
                .Verify(_ => _.ExecuteAsync(
                    It.IsAny<CancellationToken>()),
                    Times.Once);

            OutputHelper.WriteLine(instance.ToString());
        }

        [Fact]
        public async Task TestStartInstanceFaultedAsync()
        {
            _mockFileStoreService
                .Setup(_ => _.GetAssemblyPath(It.IsAny<int>()))
                .Returns(AssemblyDirectory!);

            var mockPlugIn = new Mock<IPlugIn>();

            _mockRuntimeAssemblyLoadContextFactory
                .Setup(_ => _.Create(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(_mockRuntimeAssemblyLoadContext.Object);

            _mockRuntimeAssemblyLoadContext
                .Setup(_ => _.LoadFromAssemblyName(
                    It.IsAny<AssemblyName>()))
                .Returns(typeof(object).Assembly);

            _mockPlugInFactory
                .Setup(_ => _.CreateInstance(
                    It.IsAny<Assembly>(),
                    It.IsAny<IRuntimeAssemblyLoadContext>()))
                .Returns(mockPlugIn.Object);

            mockPlugIn
                .Setup(_ => _.ExecuteAsync(It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            SetupPlugInTypes(out PlugIn plugIn, out PlugInInstance plugInInstance);

            var result = _runtimeService
                .StartInstance(plugIn, plugInInstance);

            Assert.NotNull(result);

            var instance = await WaitForState(2, ExecutionState.Faulted);

            Assert.NotNull(instance);
            Assert.NotNull(instance.Exception);
            Assert.NotNull(instance.PlugIn);
            Assert.Equal(ExecutionState.Faulted, instance.State);

            _mockPlugInFactory
                .Verify(_ => _.CreateInstance(
                    It.IsAny<Assembly>(),
                    It.IsAny<IRuntimeAssemblyLoadContext>()),
                    Times.Once);

            mockPlugIn
                .Verify(_ => _.ExecuteAsync(
                    It.IsAny<CancellationToken>()),
                    Times.Once);

            OutputHelper.WriteLine(instance.ToString());
        }

        [Fact]
        public async Task TestStartInstanceMaxRunningAsync()
        {
            for (int i = 1; i < 6; i++)
            {
                _runtimeService._executingInstances.Add(new ExecutingInstance()
                {
                    Id = i,
                    Name = i.ToString()
                });
            }

            SetupPlugInTypes(out PlugIn plugIn, out PlugInInstance plugInInstance);

            Assert.Throws<RuntimeMaxInstancesRunningException>( () => _runtimeService.StartInstance(plugIn, plugInInstance));
        }

        [Fact]
        public void TestStartInstanceRunningAsync()
        {
            _runtimeService._executingInstances.Add(new ExecutingInstance()
            {
                Id = 2,
                Name = "Test", 
                State = ExecutionState.Active
            });

            SetupPlugInTypes(out PlugIn plugIn, out PlugInInstance plugInInstance);

            var result = _runtimeService
                .StartInstance(plugIn, plugInInstance);

            Assert.NotNull(result);
            Assert.Equal(ExecutionState.Active, result.State);
        }
        [Fact]
        public async Task TestStopInstanceAsync()
        {
            var tokenSource = new CancellationTokenSource();

            var instance = new ExecutingInstance()
            {
                Id = 1,
                Name = "InstanceName",
                State = ExecutionState.Active,
                TokenSource = tokenSource
            };

            instance.Task = Task.Run(async () => await WaitTaskAsync(tokenSource.Token)).ContinueWith((antecedent) => UpdateState(antecedent, instance));

            _runtimeService._executingInstances.Add(instance);

            _runtimeService.StopInstance(1);

            var executingInstance = await WaitForState(1, ExecutionState.Complete);

            Assert.NotNull(executingInstance);
            Assert.Equal(ExecutionState.Complete, executingInstance.State);
        }

        private static void SetupPlugInTypes(out PlugIn plugIn, out PlugInInstance plugInInstance)
        {
            plugIn = new PlugIn()
            {
                Id = 1
            };
            plugIn.Package = new Package()
            {
                AssemblyFile = TestFixture.AssemblyFileName
            };

            plugInInstance = new PlugInInstance()
            {
                Id = 2,
                Name = "test"
            };
        }
        private void UpdateState(
            Task antecedent,
            ExecutingInstance executingInstance)
        {
            OutputHelper.WriteLine("Waiting Task complete");
            executingInstance.State = ExecutionState.Complete;
        }

        private async Task<ExecutingInstance?> WaitForState(int id, ExecutionState state)
        {
            int i = 0;
            ExecutingInstance? executingInstance;

            do
            {
                executingInstance = _runtimeService.GetExecutingInstance(id);

                await Task.Delay(WaitDelay);

                i++;
            } while (executingInstance != null && executingInstance.State != state && i <= WaitItterations);

            return executingInstance;
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
    }
}