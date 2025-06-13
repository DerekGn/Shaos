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

namespace Shaos.Repository.Exceptions
{
    /// <summary>
    /// An exception that is thrown when an <see cref="BaseEntity"/> type cannot be found in the repository
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Create and instance of <see cref="NotFoundException"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="BaseEntity"/> that was not found</param>
        public NotFoundException(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Create and instance of <see cref="NotFoundException"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="BaseEntity"/> that was not found</param>
        /// <param name="message">An associated message</param>
        public NotFoundException(int id,
                                 string message) : base(message)
        {
            Id = id;
        }

        /// <summary>
        /// Create and instance of <see cref="NotFoundException"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="BaseEntity"/> that was not found</param>
        /// <param name="message">An associated message</param>
        /// <param name="innerException"></param>
        public NotFoundException(int id,
                                 string message,
                                 Exception innerException) : base(message, innerException)
        {
            Id = id;
        }

        /// <summary>
        /// The identifier of the <see cref="BaseEntity"/> that was not found
        /// </summary>
        public int Id { get; }
    }
}