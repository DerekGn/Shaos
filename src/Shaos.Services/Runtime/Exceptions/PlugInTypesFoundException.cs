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

using Shaos.Sdk;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Shaos.Services.Runtime.Exceptions
{
    /// <summary>
    /// An exception that is thrown when multiple <see cref="IPlugIn"/> derived instances are found
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PlugInTypesFoundException : Exception
    {
        /// <summary>
        /// Create an instance of a <see cref="PlugInTypesFoundException"/>
        /// </summary>
        /// <param name="count">The count of <see cref="IPlugIn"/> derived types in the <see cref="Assembly"/></param>
        public PlugInTypesFoundException(int count)
        {
            Count = count;
        }

        /// <summary>
        /// Create an instance of a <see cref="PlugInTypesFoundException"/>
        /// </summary>
        /// <param name="count">The count of <see cref="IPlugIn"/> derived types in the <see cref="Assembly"/></param>
        /// <param name="message">An associated message</param>
        public PlugInTypesFoundException(int count,
                                         string message) : base(message)
        {
            Count = count;
        }

        /// <summary>
        /// Create an instance of a <see cref="PlugInTypesFoundException"/>
        /// </summary>
        /// <param name="count">The count of <see cref="IPlugIn"/> derived types in the <see cref="Assembly"/></param>
        /// <param name="message">An associated message</param>
        /// <param name="innerException">An inner exception</param>
        public PlugInTypesFoundException(int count,
                                         string message,
                                         Exception innerException) : base(message, innerException)
        {
            Count = count;
        }

        /// <summary>
        /// The number of <see cref="IPlugIn"/> types found in the <see cref="Assembly"/>
        /// </summary>
        public int Count { get; }
    }
}