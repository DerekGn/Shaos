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

namespace Shaos.Services.System
{
    /// <summary>
    /// Represents the .net framework runtime information
    /// </summary>
    public record RuntimeInformation
    {
        /// <summary>
        /// The name of the .NET installation on which an app is running.
        /// </summary>
        public string? FrameworkDescription { get; init; }

        /// <summary>
        /// The platform architecture on which the current app is running.
        /// </summary>
        public string? OsArchitecture { get; init; }

        /// <summary>
        /// The operating system on which the app is running.
        /// </summary>
        public string? OsDescription { get; init; }

        /// <summary>
        /// The process architecture of the currently running app.
        /// </summary>
        public string? ProcessArchitecture { get; init; }

        /// <summary>
        /// The platform for which the runtime was built.
        /// </summary>
        public string? RuntimeIdentifier { get; internal set; }
    }
}