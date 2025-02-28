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

using Shaos.Repository.Models;

namespace Shaos.Services.Compiler
{
    /// <summary>
    /// The <see cref="PlugIn"/> compilation status
    /// </summary>
    public class CompilationStatus
    {
        /// <summary>
        /// Indicates if this compilation is currently active
        /// </summary>
        public bool Active => CompileTask == null || CompileTask.IsCompleted;

        /// <summary>
        /// The <see cref="CancellationTokenSource"/> for the <see cref="CompileTask"/>
        /// </summary>
        public CancellationTokenSource? CancellationTokenSource { get; internal set; }

        /// <summary>
        /// The compile <see cref="Task"/>
        /// </summary>
        public Task? CompileTask { get; internal set; }

        /// <summary>
        /// The end time of this compilation
        /// </summary>
        public DateTime EndTime { get; internal set; }

        /// <summary>
        /// The <see cref="PlugIn"/> id for this compilation
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// The <see cref="PlugIn"/> being compiled
        /// </summary>
        public PlugIn PlugIn { get; init; }

        /// <summary>
        /// The start time of this compilation
        /// </summary>
        public DateTime StartTime { get; init; }
    }
}