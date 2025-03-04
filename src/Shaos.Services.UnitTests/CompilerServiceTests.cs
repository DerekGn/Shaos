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

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shaos.Services.Compiler;
using Shaos.Services.Options;
using Shaos.Services.UnitTests.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public class CompilerServiceTests : BaseUnitTests
    {
        private readonly ICompilerService _compilerService;
        private readonly ITestOutputHelper _output;

        public CompilerServiceTests(ITestOutputHelper output)
        {
            var factory = ServiceProvider.GetService<ILoggerFactory>();
            var options = ServiceProvider.GetService<IOptions<CSharpCompilerServiceOptions>>();

            _compilerService = new CSharpCompilerService(
                factory!.CreateLogger<CSharpCompilerService>(),
                options!);

            _output = output;
        }

        [Fact]
        public async Task TestCompileNotOkAsync()
        {
            using MemoryStream stream = new();

            List<string> files =
            [
                ".\\TestCode\\TestPlugInNotOk.txt"
            ];

            var result = await _compilerService.CompileAsync("Assembly.dll", stream, files);

            _output.WriteLine(result.ToFormattedString());

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.True(result.Diagnostics.Any());
            Assert.Equal(DiagnosticSeverity.Error, result.Diagnostics.First().Severity);
        }

        [Fact]
        public async Task TestCompileOkAsync()
        {
            using MemoryStream stream = new();

            List<string> files =
            [
                ".\\TestCode\\TestPlugInOk.txt"
            ];

            var result = await _compilerService.CompileAsync("Assembly.dll", stream, files);

            _output.WriteLine(result.ToFormattedString());

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Diagnostics.Any());
        }
    }
}
