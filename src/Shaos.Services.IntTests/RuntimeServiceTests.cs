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
using Shaos.Services.IO;
using Shaos.Services.Runtime;
using Shaos.Services.Shared.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.IntTests
{
    public class RuntimeServiceTests : BaseTests, IClassFixture<TestFixture>
    {
        private readonly FileStoreService _fileStoreService;
        private readonly TestFixture _fixture;
        private readonly PlugInFactory _plugInFactory;
        private readonly RuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;
        private readonly RuntimeService _runtimeService;

        public RuntimeServiceTests(ITestOutputHelper outputHelper, TestFixture fixture) : base(outputHelper)
        {
            _fixture = fixture;

            _runtimeAssemblyLoadContextFactory = new RuntimeAssemblyLoadContextFactory();

            _fileStoreService = new FileStoreService(
                LoggerFactory!.CreateLogger<FileStoreService>(),
                _fixture.FileStoreOptions);

            _plugInFactory = new PlugInFactory(
                LoggerFactory!.CreateLogger<PlugInFactory>());

            _runtimeService = new RuntimeService(
                LoggerFactory!.CreateLogger<RuntimeService>(),
                _plugInFactory,
                _fileStoreService,
                _runtimeAssemblyLoadContextFactory);
        }

        [Fact]
        public async Task TestStartThenStop()
        {
            var result = _runtimeService
                .StartInstance(2, 2, "PlugInName", TestFixture.PlugInAssembly);

            Assert.NotNull(result);

            await WaitForState(2, ExecutionState.Active);

            OutputHelper.WriteLine(result.ToString());

            if (result.State != ExecutionState.Active)
            {
                Assert.Equal(ExecutionState.Active, result.State);
            }
            else
            {
                await Task.Delay(1000);

                _runtimeService.StopInstance(result.Id);

                var instance = await WaitForState(result.Id, ExecutionState.Complete);

                Assert.Equal(ExecutionState.Complete, instance.State);
            }
        }

        private async Task<ExecutingInstance?> WaitForState(int id, ExecutionState state)
        {
            int i = 0;
            ExecutingInstance? executingInstance;

            do
            {
                executingInstance = _runtimeService.GetExecutingInstance(id);

                await Task.Delay(10);

                i++;
            } while (executingInstance != null && executingInstance.State != state && i <= 500);

            return executingInstance;
        }
    }
}