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
using Shaos.Repository.Models;
using Shaos.Sdk;
using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;
using Shaos.Sdk.Exceptions;
using Shaos.Services.Extensions;
using System.Runtime.CompilerServices;
using ModelDevice = Shaos.Repository.Models.Devices.Device;

namespace Shaos.Services
{
    /// <summary>
    /// A <see cref="IHostContext"/> implementation
    /// </summary>
    public class HostContext : IHostContext
    {
        private readonly ILogger<HostContext> _logger;
        private readonly int _instanceIdentifier;
        private readonly IRepository _repository;

        /// <summary>
        /// Create an instance of a <see cref="HostContext"/>
        /// </summary>
        /// <param name="logger">A <see cref="ILogger{T}"/> instance</param>
        /// <param name="repository">The <see cref="IRepository"/> instance</param>
        /// <param name="instanceIdentifier"></param>
        public HostContext(ILogger<HostContext> logger,
                           IRepository repository,
                           int instanceIdentifier)
        {
            _logger = logger;
            _repository = repository;
            _instanceIdentifier = instanceIdentifier;
        }

        /// <inheritdoc/>
        public async Task<Device> CreateDeviceAsync(Device device,
                                                    CancellationToken cancellationToken = default)
        {
            var plugInInstance = await _repository.GetFirstOrDefaultAsync<PlugInInstance>(_ => _.Id == _instanceIdentifier,
                                                                                          cancellationToken);

            ModelDevice modelDevice = device.ToModel();

            if (plugInInstance != null)
            {
                await _repository.AddAsync(modelDevice, cancellationToken);

                await _repository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created PlugInInstance: [{InstanceId}] Device: [{DeviceId}]",
                                       _instanceIdentifier,
                                       modelDevice.Id);
            }
            else
            {
                _logger.LogWarning("Unable to resolve [{Type}] Id: [{Identifier}]",
                                   nameof(PlugInInstance),
                                   _instanceIdentifier);

                throw new DeviceParentNotFoundException(_instanceIdentifier);
            }

            return device;
        }

        /// <inheritdoc/>
        public async Task<Device> CreateDeviceParameterAsync(int id,
                                                             BaseParameter parameter,
                                                             CancellationToken cancellationToken = default)
        {
            var device = await ResolveDeviceAsync(id, cancellationToken);

            if (device == null)
            {
                throw new DeviceNotFoundException(id);
            }
            else
            {
                device.Parameters.Add(parameter.ToModel()!);

                await _repository.SaveChangesAsync(cancellationToken);
            }

            return device.ToSdk();
        }

        /// <inheritdoc/>
        public async Task DeleteDeviceAsync(int id,
                                            CancellationToken cancellationToken = default)
        {
            var device = await ResolveDeviceAsync(id, cancellationToken);

            if (device != null)
            {
                _logger.LogInformation("Deleting [{Device}] Id: [{Identifier}] For [{Type}] Id: [{InstanceIdentifier}]",
                                       nameof(ModelDevice),
                                       id,
                                       nameof(PlugInInstance),
                                       _instanceIdentifier);

                await _repository.DeleteAsync<ModelDevice>(device.Id, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<Device> DeleteDeviceParameterAsync(int id,
                                                             int parameterId,
                                                             CancellationToken cancellationToken = default)
        {
            var device = await ResolveDeviceAsync(id, cancellationToken);

            if (device == null)
            {
                throw new DeviceNotFoundException(id);
            }
            else
            {
                var parameter = device.Parameters.FirstOrDefault(_ => _.Id == id);
                if (parameter != null)
                {
                    device.Parameters.Remove(parameter);
                }

                await _repository.SaveChangesAsync(cancellationToken);
            }

            return device.ToSdk();
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<Device> GetDevicesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var device in _repository.GetEnumerableAsync<ModelDevice>(_ => _.PlugInInstance!.Id == _instanceIdentifier,
                                                                                     withNoTracking: false,
                                                                                     cancellationToken: cancellationToken))
            {
                yield return device.ToSdk();
            }
        }

        private async Task<ModelDevice?> ResolveDeviceAsync(int id, CancellationToken cancellationToken)
        {
            var device = await _repository.GetFirstOrDefaultAsync<ModelDevice>(_ => _.Id == id && _.PlugInInstance!.Id == _instanceIdentifier,
                                                                               cancellationToken);

            if (device == null)
            {
                _logger.LogInformation("No device found Id: [{Identifier}] Instance Identifier: [{InstanceIdentifier}]", id, _instanceIdentifier);
            }

            return device;
        }
    }
}