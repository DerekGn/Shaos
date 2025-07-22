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
using Shaos.Services.Runtime.Host;
using Shaos.Services.UnitTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public class PlugInBuilderTests : PlugInBuilderBaseTests
    {
        private readonly PlugInBuilder _builder;

        public PlugInBuilderTests(ITestOutputHelper output,
                                  TestFixture fixture) : base(output, fixture)
        {
            _builder = new PlugInBuilder(LoggerFactory!,
                                         LoggerFactory!.CreateLogger<PlugInBuilder>());
        }

        [Fact]
        public void TestLoad()
        {
            var context = _unloadingWeakReference.Target;
            var assembly = context.LoadFromAssemblyPath(_fixture.AssemblyFilePath);

            var instanceConfiguration = new InstanceConfiguration(false);

            _builder.Load(assembly, instanceConfiguration);

            Assert.NotNull(_builder.PlugIn);
        }
    }
}
