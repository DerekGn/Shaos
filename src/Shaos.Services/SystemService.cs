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
using System.Diagnostics;
using System.Runtime.InteropServices;

#warning resolve assembly version

namespace Shaos.Services
{
    public class SystemService : ISystemService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<SystemService> _logger;
        private readonly Lazy<OsInformation> _osInformation;

        public SystemService(
            ILogger<SystemService> logger,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostApplicationLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));

            _osInformation = new Lazy<OsInformation>(() =>
            {
                return new OsInformation()
                {
                    FrameworkDescription = RuntimeInformation.FrameworkDescription,
                    OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
                    OsDescription = RuntimeInformation.OSDescription,
                    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
                    RuntimeIdentifier = RuntimeInformation.RuntimeIdentifier
                };
            });
        }

        public SystemEnvironment GetEnvironment()
        {
            return new SystemEnvironment()
            {
                Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                Is64BitProcess = Environment.Is64BitProcess,
                IsPrivilegedProcess = Environment.IsPrivilegedProcess,
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                ProcessId = Environment.ProcessId,
                ProcessorCount = Environment.ProcessorCount,
                SystemPageSize = Environment.SystemPageSize,
                Version = Environment.Version.ToString(),
                WorkingSet = Environment.WorkingSet
            };
        }

        public OsInformation GetOsInformation()
        {
            return _osInformation.Value;
        }

        public ProcessInformation GetProcessInformation()
        {
            var process = Process.GetCurrentProcess();

            return new ProcessInformation()
            {
                BasePriority = process.BasePriority,
                HandleCount = process.HandleCount,
                MaxWorkingSet = process.MaxWorkingSet.ToInt64(),
                MinWorkingSet = process.MinWorkingSet.ToInt64(),
                NonpagedSystemMemorySize = process.NonpagedSystemMemorySize64,
                PagedMemorySize = process.PagedMemorySize64,
                PagedSystemMemorySize = process.PagedSystemMemorySize64,
                PeakPagedMemorySize = process.PeakPagedMemorySize64,
                PeakVirtualMemorySize = process.PeakVirtualMemorySize64,
                PeakWorkingSet = process.PeakWorkingSet64,
                PrivateMemorySize = process.PrivateMemorySize64,
                PrivilegedProcessorTime = process.PrivilegedProcessorTime,
                ProcessName = process.ProcessName,
                ProcessorAffinity = process.ProcessorAffinity.ToInt64(),
                StartTime = process.StartTime,
                ThreadsCount = process.Threads.Count,
                TotalProcessorTime = process.TotalProcessorTime,
                UserProcessorTime = process.UserProcessorTime,
                VirtualMemorySize = process.VirtualMemorySize64,
            };
        }

        public string GetVersion()
        {
            return "1.0.0";
        }

        public void StopApplication()
        {
            _logger.LogInformation("Stopping Application");

            _hostApplicationLifetime.StopApplication();
        }
    }
}