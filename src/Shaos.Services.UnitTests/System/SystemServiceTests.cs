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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Shaos.Services.Shared.Tests;
using Shaos.Services.System;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.System
{
    public class SystemServiceTests : BaseTests
    {
        private readonly Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;
        private readonly SystemService _systemService;

        public SystemServiceTests(ITestOutputHelper output) : base(output)
        {
            var factory = ServiceProvider.GetService<ILoggerFactory>();

            _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();

            _systemService = new SystemService(
                factory!.CreateLogger<SystemService>(),
                _mockHostApplicationLifetime.Object);
        }

        [Fact]
        public void TestGetEnvironment()
        {
            var result = _systemService.GetEnvironment();

            Assert.NotNull(result);
        }

        [Fact]
        public void TestGetOsInformation()
        {
            var result = _systemService.GetOsInformation();

            Assert.NotNull(result);
        }

        [Fact]
        public void TestGetProcessInformation()
        {
            var result = _systemService.GetProcessInformation();

            Assert.NotNull(result);
        }

        [Fact]
        public void TestGetVersion()
        {
            var result = _systemService.GetVersion();

            Assert.NotNull(result);
        }

        [Fact]
        public void TestShutdownApplication()
        {
            _systemService.ShutdownApplication();

            _mockHostApplicationLifetime.Verify(_ => _.StopApplication(), Times.Once);
        }
    }
}