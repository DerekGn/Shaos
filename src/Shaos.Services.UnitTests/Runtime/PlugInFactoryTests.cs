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
using Shaos.Services.Runtime.Factories;
using Shaos.Services.UnitTests.Fixtures;
using Shaos.Testing.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.Runtime.Factories
{
    public class PlugInFactoryTests : BaseTests, IClassFixture<PlugInTestFixture>
    {
        private readonly PlugInTestFixture _fixture;
        private readonly PlugInFactory _plugInFactory;

        public PlugInFactoryTests(
            ITestOutputHelper output,
            PlugInTestFixture fixture) : base(output)
        {
            _fixture = fixture;

            _plugInFactory = new PlugInFactory(
                LoggerFactory!,
                LoggerFactory!.CreateLogger<PlugInFactory>());
        }

        [Fact]
        public void TestCreateInstance()
        {
#warning TODO
            //var context = _fixture.AssemblyLoadContextReference.Target;
            //var assembly = context.LoadFromAssemblyName(_fixture.AssemblyName);

            //var configuration = _plugInFactory.LoadConfiguration(assembly);
            //var plugIn = _plugInFactory.CreateInstance(assembly, configuration);

            //Assert.NotNull(plugIn);

            //context.Unload();
        }

        [Fact]
        public void TestCreateInstanceNotFound()
        {
#warning TODO
            //var assembly = typeof(object).Assembly;

            //Assert.Throws<InvalidOperationException>(() => _plugInFactory.CreateInstance(assembly));
        }
    }
}
