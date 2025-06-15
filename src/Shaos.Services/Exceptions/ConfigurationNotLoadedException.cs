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
using System.Diagnostics.CodeAnalysis;

namespace Shaos.Services.Exceptions
{
    /// <summary>
    /// Thrown when a <see cref="PlugInInstance"/> configuration can not be loaded
    /// </summary>
    /// <remarks>
    /// Create an instance of a <see cref="ConfigurationNotLoadedException"/>
    /// </remarks>
    /// <param name="id">The identifier of the <see cref="PlugInInstance"/></param>
    [ExcludeFromCodeCoverage]
    public class ConfigurationNotLoadedException(int id) : Exception
    {
        /// <summary>
        /// The identifier of the <see cref="PlugIn"/> instance
        /// </summary>
        public int Id { get; } = id;
    }
}