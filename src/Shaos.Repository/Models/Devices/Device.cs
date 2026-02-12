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

using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Repository.Models.Devices
{
    /// <summary>
    /// A device model
    /// </summary>
    public class Device : BaseEntity
    {
        /// <summary>
        /// The <see cref="Sdk.Devices.Device"/> identifier
        /// </summary>
        public int InstanceId { get; set; }

        /// <summary>
        /// The <see cref="Device"/> instance name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The set of <see cref="BaseParameter"/>
        /// </summary>
        public List<BaseParameter> Parameters { get; set; } = [];

        /// <summary>
        /// The parent <see cref="PlugInInstance"/>
        /// </summary>
        public PlugInInstance? PlugInInstance { get; set; } = null;

        /// <summary>
        /// The <see cref="PlugInInstance"/> identifier
        /// </summary>
        public int? PlugInInstanceId { get; set; } = null;
    }
}