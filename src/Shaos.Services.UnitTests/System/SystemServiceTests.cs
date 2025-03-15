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
            _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();

            _systemService = new SystemService(
                Factory!.CreateLogger<SystemService>(),
                _mockHostApplicationLifetime.Object);
        }

        [Fact]
        public void TestGetEnvironment()
        {
            var result = _systemService.GetEnvironment();

            Assert.NotNull(result);
            Assert.True(result.Is64BitOperatingSystem);
            Assert.True(result.Is64BitProcess);
            Assert.False(result.IsPrivilegedProcess);
            Assert.NotNull(result.MachineName);
            Assert.NotNull(result.OSVersion);
            Assert.NotEqual(0, result.ProcessId);
            Assert.NotEqual(0, result.ProcessorCount);
            Assert.NotEqual(0, result.SystemPageSize);
            Assert.NotNull(result.Version);
            Assert.NotEqual(0, result.WorkingSet);
        }

        [Fact]
        public void TestGetOsInformation()
        {
            var result = _systemService.GetOsInformation();

            Assert.NotNull(result);
            Assert.NotNull(result.FrameworkDescription);
            Assert.NotNull(result.OsArchitecture);
            Assert.NotNull(result.OsDescription);
            Assert.NotNull(result.ProcessArchitecture);
            Assert.NotNull(result.RuntimeIdentifier);
        }

        [Fact]
        public void TestGetProcessInformation()
        {
            var result = _systemService.GetProcessInformation();

            Assert.NotNull(result);
            Assert.True(result.BasePriority >= 0);
            Assert.True(result.HandleCount >= 0);
            Assert.True(result.MaxWorkingSet >= 0);
            Assert.True(result.MinWorkingSet >= 0);
            Assert.True(result.NonpagedSystemMemorySize >= 0);
            Assert.True(result.PagedMemorySize >= 0);
            Assert.True(result.PagedSystemMemorySize >= 0);
            Assert.True(result.PeakPagedMemorySize >= 0);
            Assert.True(result.PeakVirtualMemorySize >= 0);
            Assert.True(result.PeakWorkingSet >= 0);
            Assert.True(result.PrivateMemorySize >= 0);
            Assert.NotEqual(0, result.PrivilegedProcessorTime.TotalMilliseconds);
            Assert.NotNull(result.ProcessName);
            Assert.True(result.ProcessorAffinity >= 0);
            Assert.NotEqual(0, result.StartTime.Ticks);
            Assert.True(result.ThreadsCount >= 0);
            Assert.NotEqual(0, result.TotalProcessorTime.TotalMilliseconds);
            Assert.NotEqual(0, result.UserProcessorTime.TotalMilliseconds);
            Assert.True(result.VirtualMemorySize >= 0);
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