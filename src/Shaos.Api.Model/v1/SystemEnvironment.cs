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

namespace Shaos.Api.Model.v1
{
    /// <summary>
    /// The system environment
    /// </summary>
    public class SystemEnvironment
    {
        /// <summary>
        /// Indicates whether the current operating system is a 64-bit operating system.
        /// </summary>
        public bool Is64BitOperatingSystem { get; init; }
        /// <summary>
        /// Indicates whether the current process is a 64-bit process.
        /// </summary>
        public bool Is64BitProcess { get; init; }
        /// <summary>
        /// Indicates whether the current process is authorized to perform security-relevant functions
        /// </summary>
        public bool IsPrivilegedProcess { get; init; }
        /// <summary>
        /// Gets the NetBIOS name of this local computer.
        /// </summary>
        public string? MachineName { get; init; }
        /// <summary>
        /// The current platform identifier and version number.
        /// </summary>
        public string? OSVersion { get; init; }
        /// <summary>
        /// The unique identifier for the current process.
        /// </summary>
        public int ProcessId { get; init; }
        /// <summary>
        /// The number of processors available to the current process.
        /// </summary>
        public int ProcessorCount { get; init; }
        /// <summary>
        /// The number of bytes in the operating system's memory page.
        /// </summary>
        public int SystemPageSize { get; init; }
        /// <summary>
        /// The version of the common language runtime.
        /// </summary>
        public string? Version { get; init; }
        /// <summary>
        /// The amount of physical memory mapped to the process context.
        /// </summary>
        public long WorkingSet { get; init; }
    }
}
