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
using Shaos.Repository.Models;
using Shaos.Services.IntTests.Fixtures;
using Shaos.Services.IO;
using Shaos.Services.Runtime;
using Shaos.Services.Shared.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.IntTests
{
    public class RuntimeServiceTests : BaseRuntimeServiceTests, IClassFixture<RuntimeServiceFixture>, IDisposable
    {
        private readonly FileStoreService _fileStoreService;
        private readonly RuntimeServiceFixture _fixture;
        private readonly PlugInFactory _plugInFactory;
        private readonly RuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;
        private readonly RuntimeService _runtimeService;

        public RuntimeServiceTests(ITestOutputHelper outputHelper, RuntimeServiceFixture fixture) : base(outputHelper)
        {
            _fixture = fixture;

            _runtimeAssemblyLoadContextFactory = new RuntimeAssemblyLoadContextFactory();

            _fileStoreService = new FileStoreService(
                LoggerFactory!.CreateLogger<FileStoreService>(),
                _fixture.FileStoreOptions);

            _plugInFactory = new PlugInFactory(
                LoggerFactory!.CreateLogger<PlugInFactory>());

            var optionsInstance = new RuntimeServiceOptions();

            var options = Options.Create(optionsInstance);

            _runtimeService = new RuntimeService(
                LoggerFactory!.CreateLogger<RuntimeService>(),
                options,
                _plugInFactory,
                _fileStoreService,
                _runtimeAssemblyLoadContextFactory);

            _runtimeService.InstanceStateChanged += RuntimeServiceInstanceStateChanged;
        }

        public void Dispose()
        {
            _runtimeService.InstanceStateChanged -= RuntimeServiceInstanceStateChanged;
        }

        [Fact]
        public async Task TestStartThenStop()
        {
            SetupPlugInTypes(out PlugIn plugIn, out PlugInInstance plugInInstance);

            SetupStateWait(InstanceState.Active);

            var result = _runtimeService
                .StartInstance(plugIn, plugInInstance);

            Assert.True(WaitForStateChange());

            Assert.NotNull(result);

            if (result.State != InstanceState.Active)
            {
                Assert.Equal(InstanceState.Active, result.State);
            }
            else
            {
                await Task.Delay(1000);

                SetupStateWait(InstanceState.Complete);

                _runtimeService.StopInstance(result.Id);

                Assert.True(WaitForStateChange());

                Assert.Equal(InstanceState.Complete, result!.State);
            }
        }
    }
}