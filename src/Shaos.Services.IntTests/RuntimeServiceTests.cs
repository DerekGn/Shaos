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
using Shaos.Services.IO;
using Shaos.Services.Runtime;
using Shaos.Services.Shared.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.IntTests
{
    public class RuntimeServiceTests : BaseTests
    {
        private readonly RuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;
        private readonly FileStoreService _fileStoreService;
        private readonly RuntimeService _runtimeService;
        private readonly PlugInFactory _plugInFactory;

        public RuntimeServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            var optionsInstance = new FileStoreOptions()
            {
                BinariesPath = string.Concat(AssemblyDirectory!.Replace("Shaos.Services.IntTests", "Shaos.Test.PlugIn"), "\\output"),
                PackagesPath = ""
            };

            IOptions<FileStoreOptions> options = Options.Create(optionsInstance);
            
            _runtimeAssemblyLoadContextFactory = new RuntimeAssemblyLoadContextFactory();

            _fileStoreService = new FileStoreService(
                LoggerFactory!.CreateLogger<FileStoreService>(),
                options!);

            _plugInFactory = new PlugInFactory(
                LoggerFactory!.CreateLogger<PlugInFactory>());

            _runtimeService = new RuntimeService(
                LoggerFactory!.CreateLogger<RuntimeService>(),
                _plugInFactory,
                _fileStoreService,
                _runtimeAssemblyLoadContextFactory);
        }

        [Fact]
        public async Task TestPlugInStartThenStop()
        {
            var result = _runtimeService
                .StartInstance(1, 2, "PlugInName", "Shaos.Test.PlugIn.dll");

            Assert.NotNull(result);

            int i = 0;
            ExecutingInstance? executingInstance;

            do
            {
                executingInstance = _runtimeService.GetExecutingInstance(result.Id);

                await Task.Delay(10);

                i++;

            } while (executingInstance != null && executingInstance.State != ExecutionState.Active && i <= 500);


            if (result.State != ExecutionState.Active)
            {
                OutputHelper.WriteLine(result.ToString());

                Assert.Equal(ExecutionState.Active, result.State);
            }
            else
            {
                await _runtimeService.StopInstanceAsync(2, "name");
            }
        }
    }
}