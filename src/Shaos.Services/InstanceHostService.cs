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
using Microsoft.Extensions.Primitives;
using Shaos.Repository.Models;
using Shaos.Services.Exceptions;
using Shaos.Services.Repositories;
using Shaos.Services.Runtime;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Host;
using System.Collections.Specialized;
using System.Text.Json;

namespace Shaos.Services
{
    public class InstanceHostService : IInstanceHostService
    {
        private readonly IInstanceHost _instanceHost;
        private readonly ILogger<InstanceHostService> _logger;
        private readonly IPlugInInstanceRepository _plugInInstanceRepository;

        public InstanceHostService(
            ILogger<InstanceHostService> logger,
            IInstanceHost instanceHost,
            IPlugInInstanceRepository plugInInstanceRepository)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(instanceHost);
            ArgumentNullException.ThrowIfNull(plugInInstanceRepository);

            _logger = logger;
            _instanceHost = instanceHost;
            _plugInInstanceRepository = plugInInstanceRepository;
        }

        public async Task<object?> GetInstanceConfigurationAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteInstanceOperationAsync(id, async (instance) =>
            {
                object? instanceConfiguration = null;

                if (instance.State != InstanceState.Loaded)
                {
                    throw new InstanceInvalidStateException(id, instance.State);
                }

                var plugInInstance = await LoadPlugInInstanceAsync(id, cancellationToken) ?? throw new PlugInInstanceNotFoundException(id);

                var package = plugInInstance?.PlugIn?.Package;

                if (package != null && package.HasConfiguration)
                {
                    if (!string.IsNullOrEmpty(plugInInstance?.Configuration))
                    {
                        instanceConfiguration = JsonSerializer.Deserialize<object>(plugInInstance.Configuration);
                    }
                    else
                    {
                        instanceConfiguration = instance.Context.PlugInConfiguration;
                    }
                }

                return instanceConfiguration;
            });
        }

        /// <inheritdoc/>
        public async Task StartInstanceAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            await ExecuteInstanceOperationAsync(id, async (instance) =>
            {
                var plugInInstance = await LoadPlugInInstanceAsync(id, cancellationToken);

                if (plugInInstance != null)
                {
                    var package = plugInInstance.PlugIn.Package;
                    object? plugInConfiguration = null;

                    if (package != null && package.HasConfiguration)
                    {
                        if (!string.IsNullOrEmpty(plugInInstance.Configuration))
                        {
                            plugInConfiguration = JsonSerializer.Deserialize<object>(plugInInstance.Configuration);
}
                        else
                        {
                            throw new PlugInInstanceNotConfiguredException(id);
                        }
                    }

                    _instanceHost.StartInstance(id);
                }
                else
                {
                    _logger.LogWarning("Unable to start a PlugIn instance. PlugIn instance Id: [{Id}] was not found.", id);
                }
            });
        }

        /// <inheritdoc/>
        public void StopInstance(int id)
        {
            ExecuteInstanceOperation(id, (instance) =>
            {
                _instanceHost.StopInstance(id);
            });
        }

        /// <inheritdoc/>
        public async Task UpdateInstanceConfigurationAsync(
            int id,
            IEnumerable<KeyValuePair<string, StringValues>> collection,
            CancellationToken cancellationToken = default)
        {
            await ExecuteInstanceOperationAsync(id, async (instance) =>
            {
            });
        }

        private void ExecuteInstanceOperation(int id, Action<Instance> operation)
        {
            Instance? instance = _instanceHost.Instances.FirstOrDefault(_ => _.Id == id) ?? throw new InstanceNotFoundException(id);

            operation(instance);
        }

        private async Task ExecuteInstanceOperationAsync(int id, Func<Instance, Task> operation)
        {
            Instance? instance = _instanceHost.Instances.FirstOrDefault(_ => _.Id == id) ?? throw new InstanceNotFoundException(id);

            await operation(instance);
        }

        private async Task<T> ExecuteInstanceOperationAsync<T>(int id, Func<Instance, Task<T>> operation)
        {
            Instance? instance = _instanceHost.Instances.FirstOrDefault(_ => _.Id == id) ?? throw new InstanceNotFoundException(id);

            return await operation(instance);
        }

        private async Task<PlugInInstance> LoadPlugInInstanceAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _plugInInstanceRepository.GetByIdAsync(
                id,
                includeProperties: [nameof(PlugIn), $"{nameof(PlugIn)}.{nameof(Package)}"],
                cancellationToken: cancellationToken);
        }
    }
}