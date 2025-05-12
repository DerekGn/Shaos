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
using Shaos.Repository.Models;
using Shaos.Sdk;
using Shaos.Services.Exceptions;
using Shaos.Services.Repositories;
using Shaos.Services.Runtime.Host;
using System.Text.Json;

namespace Shaos.Services
{
    /// <summary>
    ///
    /// </summary>
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

        /// <inheritdoc/>
        public async Task StartInstanceAsync(int id, CancellationToken cancellationToken = default)
        {
            if (_instanceHost.InstanceExists(id))
            {
                var plugInInstance = await LoadPlugInInstanceAsync(id, cancellationToken);

                if(plugInInstance != null)
                {
                    var package = plugInInstance.PlugIn.Package;
                    object? plugInConfiguration = null;

                    if (package != null && package.HasConfiguration)
                    {
                        if (!string.IsNullOrEmpty(plugInInstance.Configuration))
                        {
                            plugInConfiguration = JsonSerializer.Deserialize<BasePlugInConfiguration>(plugInInstance.Configuration);
                        }
                        else
                        {
                            throw new PlugInInstanceNotConfiguredException(id);
                        }
                    }

                    _instanceHost.StartInstance(id, plugInConfiguration);
                }
                else
                {
                    _logger.LogWarning("Unable to start a PlugIn instance. PlugIn instance Id: [{Id}] was not found.", id);
                }
            }
            else
            {
                _logger.LogWarning("Unable to start a PlugIn instance. Instance host does not contain instance Id: [{Id}]", id);
            }
        }

        /// <inheritdoc/>
        public void StopInstance(int id)
        {
            if (_instanceHost.InstanceExists(id))
            {
                _instanceHost.StopInstance(id);
            }
            else
            {
                _logger.LogWarning("Unable to stop a PlugIn instance. PlugIn instance Id: [{Id}] was not found.", id);
            }
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