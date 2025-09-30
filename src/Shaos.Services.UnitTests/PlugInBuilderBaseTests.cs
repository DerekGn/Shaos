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
using Shaos.Services.Runtime;
using Shaos.Testing.Shared;
using Xunit.Abstractions;
using Xunit;
using Shaos.Services.UnitTests.Fixtures;

namespace Shaos.Services.UnitTests
{
    public abstract class PlugInBuilderBaseTests : BaseTests, IClassFixture<TestFixture>
    {
        internal readonly UnloadingWeakReference<RuntimeAssemblyLoadContext> _unloadingWeakReference;
        internal readonly TestFixture _fixture;

        protected PlugInBuilderBaseTests(ITestOutputHelper outputHelper,
                                         TestFixture fixture) : base(outputHelper)
        {
            _fixture = fixture;

            var assemblyLoadContext = new RuntimeAssemblyLoadContext(LoggerFactory!.CreateLogger<RuntimeAssemblyLoadContext>(), _fixture.AssemblyFilePath);
            _unloadingWeakReference = new UnloadingWeakReference<RuntimeAssemblyLoadContext>(assemblyLoadContext);
        }
    }
}
