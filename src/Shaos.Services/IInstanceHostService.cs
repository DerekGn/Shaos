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

using Shaos.Services.Runtime.Exceptions;

namespace Shaos.Services
{
    /// <summary>
    /// Defines a service for instance hosting service
    /// </summary>
    public interface IInstanceHostService
    {
        /// <summary>
        /// Load a PlugIn instance configuration
        /// </summary>
        /// <param name="id">The identifier of the PlugIn instance</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The configuration instance</returns>
        Task<object?> LoadInstanceConfigurationAsync(int id,
                                                     CancellationToken cancellationToken = default);

        /// <summary>
        /// Start a PlugIn instance
        /// </summary>
        /// <param name="id">The identifier of the PlugIn instance</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <exception cref="InstanceNotFoundException">Thrown if the instance was not found</exception>
        Task StartInstanceAsync(int id,
                                CancellationToken cancellationToken = default);

        /// <summary>
        /// Start all enabled PlugIn instances.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        Task StartInstancesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop a running PlugIn instance
        /// </summary>
        /// <param name="id">The identifier of the PlugIn instance</param>
        void StopInstance(int id);

        /// <summary>
        /// Update the instance configuration
        /// </summary>
        /// <param name="id">The identifier of the PlugIn instance</param>
        /// <param name="collection"></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        Task UpdateInstanceConfigurationAsync(int id,
                                              IEnumerable<KeyValuePair<string, string>> collection,
                                              CancellationToken cancellationToken = default);
    }
}