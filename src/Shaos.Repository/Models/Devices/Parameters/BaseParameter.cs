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

using Shaos.Sdk.Devices.Parameters;

namespace Shaos.Repository.Models.Devices.Parameters
{
    /// <summary>
    /// A device base parameter
    /// </summary>
    public abstract class BaseParameter : BaseEntity
    {
        protected BaseParameter(int id)
        {
            Id = id;
        }

        /// <summary>
        /// The parent <see cref="Device"/>
        /// </summary>
        public Device? Device { get; set; }

        /// <summary>
        /// The parent <see cref="Device"/> identifier
        /// </summary>
        public int? DeviceId { get; set; }

        /// <summary>
        /// The <see cref="BaseParameter"/> name
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// The <see cref="ParameterType"/>
        /// </summary>
        public ParameterType? ParameterType { get; }

        /// <summary>
        /// The <see cref="BaseParameter"/> units
        /// </summary>
        public string? Units { get; }
    }
}