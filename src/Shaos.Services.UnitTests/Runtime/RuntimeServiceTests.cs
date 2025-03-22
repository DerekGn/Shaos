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
using Moq;
using Shaos.Sdk;
using Shaos.Services.IO;
using Shaos.Services.Runtime;
using Shaos.Services.Shared.Tests;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.Runtime
{
    public class RuntimeServiceTests : BaseTests
    {
        private const int WaitDelay = 10;
        private const int WaitItterations = 500;

        private readonly Mock<IFileStoreService> _mockFileStoreService;
        private readonly Mock<IPlugInFactory> _mockPlugInFactory;
        private readonly Mock<IRuntimeAssemblyLoadContext> _mockRuntimeAssemblyLoadContext;
        private readonly Mock<IRuntimeAssemblyLoadContextFactory> _mockRuntimeAssemblyLoadContextFactory;
        private readonly RuntimeService _runtimeService;

        public RuntimeServiceTests(ITestOutputHelper output) : base(output)
        {
            _mockRuntimeAssemblyLoadContextFactory = new Mock<IRuntimeAssemblyLoadContextFactory>();
            _mockRuntimeAssemblyLoadContext = new Mock<IRuntimeAssemblyLoadContext>();
            _mockFileStoreService = new Mock<IFileStoreService>();
            _mockPlugInFactory = new Mock<IPlugInFactory>();

            _runtimeService = new RuntimeService(
                LoggerFactory!.CreateLogger<RuntimeService>(),
                _mockPlugInFactory.Object,
                _mockFileStoreService.Object,
                _mockRuntimeAssemblyLoadContextFactory.Object);
        }

        [Fact]
        public void TestGetExecutingInstanceFound()
        {
            _runtimeService._executingInstances.Add(new ExecutingInstance()
            {
                Id = 1
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
            _runtimeService._executingInstances.Add(new ExecutingInstance()
            {
                Id = 1
            });

            _runtimeService._executingInstances.Add(new ExecutingInstance()
            {
                Id = 2
            });

            _runtimeService._executingInstances.Add(new ExecutingInstance()
            {
                Id = 3
            });

            var result = _runtimeService.GetExecutingInstances();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task TestStartInstanceAsync()
        {
            var assemblyDirectory = AssemblyDirectory!.Replace("Shaos.Services.UnitTests", "Shaos.Test.PlugIn");

            OutputHelper.WriteLine("AssemblyDirectory: [{0}]", assemblyDirectory);

            _mockFileStoreService
                .Setup(_ => _.GetAssemblyPathForPlugIn(It.IsAny<int>()))
                .Returns(assemblyDirectory);

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

            var result = _runtimeService.StartInstance(
                1,
                2,
                "PlugInName",
                "Shaos.Test.PlugIn.dll");

            Assert.NotNull(result);
            Assert.Equal(ExecutionState.None, result.State);

            int i = 0;
            ExecutingInstance? executingInstance;

            do
            {
                executingInstance = _runtimeService._executingInstances.FirstOrDefault(_ => _.Id == 2);

                await Task.Delay(WaitDelay);

                i++;
            } while (executingInstance != null && executingInstance.State != ExecutionState.Complete && i <= WaitItterations);

            Assert.NotNull(executingInstance);
            Assert.NotNull(executingInstance.PlugIn);
            Assert.Equal(ExecutionState.Complete, executingInstance.State);

            _mockPlugInFactory
                .Verify(_ => _.CreateInstance(
                    It.IsAny<Assembly>(),
                    It.IsAny<IRuntimeAssemblyLoadContext>()),
                    Times.Once);

            mockPlugIn
                .Verify(_ => _.ExecuteAsync(
                    It.IsAny<CancellationToken>()),
                    Times.Once);

            OutputHelper.WriteLine(executingInstance.ToString());
        }

        [Fact]
        public async Task TestStartInstancePlugInFaultedAsync()
        {
            var assemblyDirectory = AssemblyDirectory!.Replace("Shaos.Services.UnitTests", "Shaos.Test.PlugIn");

            OutputHelper.WriteLine("AssemblyDirectory: [{0}]", assemblyDirectory);

            _mockFileStoreService
                .Setup(_ => _.GetAssemblyPathForPlugIn(It.IsAny<int>()))
                .Returns(assemblyDirectory);

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

            var result = _runtimeService.StartInstance(
                1,
                2,
                "PlugInName",
                "Shaos.Test.PlugIn.dll");

            Assert.NotNull(result);
            Assert.Equal(ExecutionState.None, result.State);

            int i = 0;
            ExecutingInstance? executingInstance;

            do
            {
                executingInstance = _runtimeService._executingInstances.FirstOrDefault(_ => _.Id == 2);

                await Task.Delay(WaitDelay);

                i++;
            } while (executingInstance != null && executingInstance.State != ExecutionState.Faulted && i <= WaitItterations);

            Assert.NotNull(executingInstance);
            Assert.NotNull(executingInstance.Exception);
            Assert.NotNull(executingInstance.PlugIn);
            Assert.Equal(ExecutionState.Faulted, executingInstance.State);

            _mockPlugInFactory
                .Verify(_ => _.CreateInstance(
                    It.IsAny<Assembly>(),
                    It.IsAny<IRuntimeAssemblyLoadContext>()),
                    Times.Once);

            mockPlugIn
                .Verify(_ => _.ExecuteAsync(
                    It.IsAny<CancellationToken>()),
                    Times.Once);

            OutputHelper.WriteLine(executingInstance.ToString());
        }

        [Fact]
        public void TestStartInstanceRunningAsync()
        {
            _runtimeService._executingInstances.Add(new ExecutingInstance()
            {
                Id = 2,
                State = ExecutionState.Active
            });

            var result = _runtimeService.StartInstance(1, 2, "name", "Shaos.Test.PlugIn.dll");

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

            int i = 0;
            ExecutingInstance? executingInstance;

            do
            {
                executingInstance = _runtimeService._executingInstances.FirstOrDefault(_ => _.Id == 1);

                await Task.Delay(WaitDelay);

                i++;
            } while (executingInstance != null && executingInstance.State != ExecutionState.Complete && i <= WaitItterations);

            Assert.NotNull(executingInstance);
            Assert.Equal(ExecutionState.Complete, executingInstance.State);
        }

        private void UpdateState(
            Task antecedent,
            ExecutingInstance executingInstance)
        {
            OutputHelper.WriteLine("Waiting Task complete");
            executingInstance.State = ExecutionState.Complete;
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