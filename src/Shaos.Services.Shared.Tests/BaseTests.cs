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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Xunit.Abstractions;

namespace Shaos.Services.Shared.Tests
{
    public abstract class BaseTests
    {
        protected BaseTests(ITestOutputHelper outputHelper)
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging((builder) => {
                    builder.AddXUnit(outputHelper);
                    builder.SetMinimumLevel(LogLevel.Debug);
                }).AddOptions();

            ServiceProvider = serviceCollection.BuildServiceProvider();

            LoggerFactory = ServiceProvider.GetService<ILoggerFactory>();

            AssemblyDirectory = Path
                .GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            OutputHelper = outputHelper;
        }

        public string? AssemblyDirectory { get; }
        public ILoggerFactory? LoggerFactory { get; }
        public ITestOutputHelper OutputHelper { get; }
        public ServiceProvider ServiceProvider { get; }
    }
}