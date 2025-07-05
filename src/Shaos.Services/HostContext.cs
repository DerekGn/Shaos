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

using Microsoft.Extensions.Logging;
using Shaos.Repository;
using Shaos.Sdk;
using Shaos.Sdk.Devices;
using System.Runtime.CompilerServices;

namespace Shaos.Services
{
    /// <summary>
    /// A <see cref="IHostContext"/> implementation
    /// </summary>
    public class HostContext : IHostContext
    {
        private readonly ILogger<HostContext> _logger;
        private readonly IShaosRepository _repository;
        private readonly int _plugInInstanceIdentifier;

        /// <summary>
        /// Create an instance of a <see cref="HostContext"/>
        /// </summary>
        /// <param name="logger">A <see cref="ILogger{T}"/> instance</param>
        /// <param name="repository">The <see cref="IShaosRepository"/> instance</param>
        /// <param name="plugInInstanceIdentifier"></param>
        public HostContext(ILogger<HostContext> logger,
                           IShaosRepository repository,
                           int plugInInstanceIdentifier)
        {
            _logger = logger;
            _repository = repository;
            _plugInInstanceIdentifier = plugInInstanceIdentifier;
        }

        public Task<Device> CreateDeviceAsync(Device device, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDeviceAsync(int identifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<Device> GetDevicesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
