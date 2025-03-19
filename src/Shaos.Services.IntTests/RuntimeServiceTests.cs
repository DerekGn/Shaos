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
        private readonly FileStoreService _fileStoreService;
        private readonly RuntimeService _runtimeService;

        public RuntimeServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            var optionsInstance = new FileStoreOptions()
            {
                BinariesPath = AssemblyDirectory!.Replace("Shaos.Services.IntTests", "Shaos.Test.PlugIn"),
                PackagesPath = ""
            };

            IOptions<FileStoreOptions> options = Options.Create(optionsInstance);

            _fileStoreService = new FileStoreService(
                Factory!.CreateLogger<FileStoreService>(),
                options!);

            _runtimeService = new RuntimeService(
                Factory!.CreateLogger<RuntimeService>(),
                _fileStoreService);
        }

        [Fact]
        public async Task TestPlugInStartThenStop()
        {
            var result = _runtimeService
                .StartInstance(1, 2, "PlugInName", "Shaos.Test.PlugIn.dll");

            Assert.NotNull(result);

            await Task.Delay(TimeSpan.FromSeconds(1));

            if(result.State == ExecutionState.Active)
            {
                await _runtimeService.StopInstanceAsync(2, "name");
            }
            else
            {
                OutputHelper.WriteLine(result.ToString());

                Assert.Equal(ExecutionState.Active, result.State);
            }
        }
    }
}