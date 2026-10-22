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

using Shaos.Repository.Exceptions;
using Shaos.Repository.Models;
using Shaos.Services.Exceptions;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Host;

namespace Shaos.Services
{
    /// <summary>
    /// Defines a service for instance hosting service
    /// </summary>
    public interface IInstanceHostService
    {
        /// <summary>
        /// Load a <see cref="PlugInInstance"/> configuration
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugInInstance"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The configuration instance</returns>
        /// <exception cref="PlugInInstanceNotFoundException">Thrown when the <see cref="PlugInInstance"/> cannot be found</exception>
        /// <exception cref="PlugInInstanceNotConfiguredException">Thrown when the <see cref="PlugInInstance"/> does not have a configuration to load</exception>
        Task<object?> LoadInstanceConfigurationAsync(int id,
                                                     CancellationToken cancellationToken = default);

        /// <summary>
        /// Start a <see cref="PlugInInstance"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugInInstance"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <exception cref="RuntimeInstanceNotFoundException">Throw if the <see cref="RuntimeInstance"/> was not found</exception>
        /// <exception cref="PlugInInstanceNotFoundException">Throw if the <see cref="PlugInInstance"/> was not found</exception>
        /// <exception cref="PlugInInformationNotAssignedException">Throw if the <see cref="PlugInInstance"/> has no <see cref="PlugInInformation"/> assigned</exception>
        /// <exception cref="PlugInInstanceNotConfiguredException">Throw if the <see cref="PlugInInstance"/> has not been configured</exception>
        Task StartInstanceAsync(int id,
                                CancellationToken cancellationToken = default);

        /// <summary>
        /// Start all enabled <see cref="PlugInInstance"/>
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        Task StartInstancesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop a running <see cref="PlugInInstance"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugInInstance"/></param>
        void StopInstance(int id);

        /// <summary>
        /// Update the <see cref="PlugInInstance"/> configuration
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugInInstance"/></param>
        /// <param name="collection">The key </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        Task UpdateInstanceConfigurationAsync(int id,
                                              IEnumerable<KeyValuePair<string, string>> collection,
                                              CancellationToken cancellationToken = default);
    }
}