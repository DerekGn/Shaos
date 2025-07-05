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

using Shaos.Sdk.Devices;

namespace Shaos.Sdk
{
    /// <summary>
    /// Defines a host context interface
    /// </summary>
    public interface IHostContext
    {
        /// <summary>
        /// Create a <see cref="Device"/> instance
        /// </summary>
        /// <param name="device">The <see cref="Device"/> instance to create</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The created <see cref="Device"/> instance</returns>
        Task<Device> CreateDeviceAsync(Device device, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a <see cref="Device"/> instance
        /// </summary>
        /// <param name="identifier">The identifier of the <see cref="Device"/> to delete</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        Task DeleteDeviceAsync(int identifier, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="IAsyncEnumerable{T}"/> of <see cref="Device"/> instances
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns></returns>
        IAsyncEnumerable<Device> GetDevicesAsync(CancellationToken cancellationToken = default);
    }
}
