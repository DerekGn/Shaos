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

namespace Shaos.Services
{
    /// <summary>
    /// The current executing process information
    /// </summary>
    public class ProcessInformation
    {
        /// <summary>
        ///  Gets the base priority of the associated process.
        /// </summary>
        public int BasePriority { get; init; }

        /// <summary>
        /// The number of handles opened by the process.
        /// </summary>
        public int HandleCount { get; init; }

        /// <summary>
        /// The maximum allowable working set size, in bytes, for the associated process.
        /// </summary>
        public long MaxWorkingSet { get; init; }

        /// <summary>
        /// The minimum allowable working set size, in bytes, for the associated process.
        /// </summary>
        public long MinWorkingSet { get; init; }

        /// <summary>
        /// The amount of nonpaged system memory, in bytes, allocated for the associated process.
        /// </summary>
        public long NonpagedSystemMemorySize { get; init; }

        /// <summary>
        /// The amount of paged memory, in bytes, allocated for the associated process.
        /// </summary>
        public long PagedMemorySize { get; init; }

        /// <summary>
        /// The amount of pageable system memory, in bytes, allocated for the associated process.
        /// </summary>
        public long PagedSystemMemorySize { get; init; }

        /// <summary>
        /// The maximum amount of memory in the virtual memory paging file, in bytes, used by the associated process.
        /// </summary>
        public long PeakPagedMemorySize { get; init; }

        /// <summary>
        /// The maximum amount of virtual memory, in bytes, used by the associated process.
        /// </summary>
        public long PeakVirtualMemorySize { get; init; }

        /// <summary>
        /// The maximum amount of physical memory, in bytes, used by the associated process.
        /// </summary>
        public long PeakWorkingSet { get; init; }

        /// <summary>
        /// The amount of private memory, in bytes, allocated for the associated process.
        /// </summary>
        public long PrivateMemorySize { get; init; }

        /// <summary>
        /// The privileged processor time for this process.
        /// </summary>
        public TimeSpan PrivilegedProcessorTime { get; init; }

        /// <summary>
        /// The name of the process.
        /// </summary>
        public string? ProcessName { get; init; }

        /// <summary>
        /// The processors on which the threads in this process can be scheduled to run.
        /// </summary>
        public long ProcessorAffinity { get; init; }

        /// <summary>
        /// The time that the associated process was started.
        /// </summary>
        public DateTime StartTime { get; init; }

        /// <summary>
        /// The of threads that are running in the associated process.
        /// </summary>
        public int ThreadsCount { get; init; }

        /// <summary>
        /// The total processor time for this process.
        /// </summary>
        public TimeSpan TotalProcessorTime { get; init; }

        /// <summary>
        /// The user processor time for this process.
        /// </summary>
        public TimeSpan UserProcessorTime { get; init; }

        /// <summary>
        /// The amount of physical memory, in bytes, allocated for the associated process.
        /// </summary>
        public long VirtualMemorySize { get; init; }
    }
}