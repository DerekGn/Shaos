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
using Shaos.Services.IO;
using Shaos.Services.Repositories;

namespace Shaos.Services.Runtime
{
    public class RuntimeService : IRuntimeService
    {
        private readonly IFileStoreService _fileStoreService;
        private readonly IInstanceHost _instanceHost;
        private readonly ILogger<RuntimeService> _logger;
        private readonly IPlugInRepository _repository;

        public RuntimeService(
            ILogger<RuntimeService> logger,
            IInstanceHost instanceHost,
            IPlugInRepository repository,
            IFileStoreService fileStoreService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(instanceHost);
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(fileStoreService);

            _logger = logger;
            _instanceHost = instanceHost;
            _repository = repository;
            _fileStoreService = fileStoreService;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Instance>> GetInstancesAsync(
            CancellationToken cancellationToken = default)
        {
            var plugIns = _repository
                .GetAsync(
                    _ => _.Package != null,
                    includeProperties: [nameof(Package), nameof(PlugIn.Instances)],
                    cancellationToken: cancellationToken
                );

            await foreach (var plugIn in plugIns)
            {
                foreach (var instance in plugIn.Instances)
                {
                    if (!_instanceHost.Instances.Any(_ => _.Id == instance.Id))
                    {
                        var assemblyFilePath = Path
                            .Combine(_fileStoreService
                            .GetAssemblyPath(instance.Id), plugIn.Package!.AssemblyFile);

                        _instanceHost.AddInstance(
                            instance.Id,
                            instance.Name,
                            assemblyFilePath);
                    }
                }
            }

            return _instanceHost.Instances;
        }

        /// <inheritdoc/>
        public async Task StartEnabledInstancesAsync(CancellationToken cancellationToken = default)
        {
            var plugIns = _repository
                .GetAsync(
                    _ => _.Package != null,
                    includeProperties: [nameof(Package), nameof(PlugIn.Instances)],
                    cancellationToken: cancellationToken
                );

            await foreach (var plugIn in plugIns)
            {
                foreach (var instance in plugIn.Instances)
                {
                    if (!_instanceHost.Instances.Any(_ => _.Id == instance.Id))
                    {
                        var assemblyFilePath = Path
                            .Combine(_fileStoreService
                            .GetAssemblyPath(instance.Id), plugIn.Package!.AssemblyFile);

                        _logger.LogInformation("Adding Instance [{Id}] Name: [{Name}] to InstanceHost",
                            instance.Id,
                            instance.Name);

                        _instanceHost.AddInstance(
                            instance.Id,
                            instance.Name,
                            assemblyFilePath);

                        if (instance.Enabled)
                        {
                            _logger.LogInformation("Starting Instance [{Id}] Name: [{Name}]",
                                instance.Id,
                                instance.Name);

                            _instanceHost.StartInstance(instance.Id);
                        }
                    }
                }
            }
        }
    }
}