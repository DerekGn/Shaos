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
        private readonly Mock<IFileStoreService> _mockFileStoreService;
        private readonly RuntimeService _runtimeService;

        public RuntimeServiceTests(ITestOutputHelper output) : base(output)
        {
            _mockFileStoreService = new Mock<IFileStoreService>();

            _runtimeService = new RuntimeService(
                Factory!.CreateLogger<RuntimeService>(),
                _mockFileStoreService.Object);
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
        public async Task TestStartInstanceAsync()
        {
            _mockFileStoreService.Setup(_ => _.GetAssemblyPathForPlugIn(It.IsAny<int>()))
                .Returns(AssemblyDirectory.Replace("Shaos.Services.UnitTests", "Shaos.Test.PlugIn"));

            var result = await _runtimeService.StartInstanceAsync(1, 2, "name", "Shaos.Test.PlugIn.dll");

            Assert.NotNull(result);
            Assert.Equal(ExecutionState.Starting, result.State);

            await Task.Delay(100);

            var executingInstance = _runtimeService._executingInstances.FirstOrDefault(_ => _.Id == 2);

            Assert.NotNull(executingInstance);
            Assert.NotNull(executingInstance.PlugIn);
            Assert.NotNull(executingInstance.Assembly);
            Assert.NotNull(executingInstance.AssemblyLoadContext);
            Assert.Equal(ExecutionState.Active, executingInstance.State);
        }

        [Fact]
        public async Task TestStartInstanceRunningAsync()
        {
            _runtimeService._executingInstances.Add(new ExecutingInstance()
            {
                Id = 2,
                State = ExecutionState.Active
            });

            var result = await _runtimeService.StartInstanceAsync(1, 2, "name", "Shaos.Test.PlugIn.dll");

            Assert.NotNull(result);
            Assert.Equal(ExecutionState.Active, result.State);
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().Location;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}