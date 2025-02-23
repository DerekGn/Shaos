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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shaos.Api.Model.v1;
using Shaos.Repository;
using Shaos.Services.Extensions;
using System.Runtime.CompilerServices;
using ApiPlugIn = Shaos.Api.Model.v1.PlugIn;
using ModelPlugIn = Shaos.Repository.Models.PlugIn;

namespace Shaos.Services
{
    public class PlugInService : IPlugInService
    {
        private readonly ShaosDbContext _context;
        private readonly IPlugInManager _manager;
        private readonly ILogger<PlugInService> _logger;

        public PlugInService(
            ILogger<PlugInService> logger,
            IPlugInManager manager,
            ShaosDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInAsync(
            string name,
            string? description,
            CancellationToken cancellationToken)
        {
            var plugIn = new ModelPlugIn()
            {
                Name = name,
                Description = description,
                IsEnabled = false
            };

            await _context.PlugIns.AddAsync(plugIn);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("PlugIn [{id}] [{name}] Created", plugIn.Id, plugIn.Name);

            return plugIn.Id;
        }

        /// <inheritdoc/>
        public async Task DeletePlugInAsync(
            int id,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("PlugIn [{id}] Deleting", id);

            // this is EF COre 7 enhancement performs select and delete in one operation
            await _context.PlugIns.Where(_ => _.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ApiPlugIn?> GetPlugInByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            ModelPlugIn? plugin = await GetPlugInByIdFromContextAsync(id, cancellationToken);

            return plugin?.ToApiModel();
        }

        private async Task<ModelPlugIn?> GetPlugInByIdFromContextAsync(
            int id,
            CancellationToken cancellationToken)
        {
            return await _context
                .PlugIns
                .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ApiPlugIn?> GetPlugInByNameAsync(
            string name,
            CancellationToken cancellationToken)
        {
            var plugin = await _context
                .PlugIns
                .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.Name == name, cancellationToken);

            return plugin?.ToApiModel();
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ApiPlugIn> GetPlugInsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in _context.PlugIns
                .AsNoTracking()
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken))
            {
                yield return item.ToApiModel();
            }
        }

        /// <inheritdoc/>
        public async Task<PlugInStatus> GetPlugInStatusAsync(
            int id,
            CancellationToken cancellationToken)
        {
#warning implement
            return new PlugInStatus();
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<PlugInStatus> GetPlugInStatusesAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            List<PlugInStatus> plugInStatuses = new List<PlugInStatus>();

            foreach(var p in plugInStatuses)
            {
                yield return p;
            } 
        }

        /// <inheritdoc/>
        public async Task SetPlugInEnabledStateAsync(
            int id,
            bool isEnabled,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Setting PlugIn [{id}] State To [{isEnabled}]", id, isEnabled);

            await UpdatePlugInAsync(
                id,
                (plugIn) =>
                {
                    if (plugIn != null)
                    {
                        plugIn.IsEnabled = isEnabled;
                    }
                },
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task StartPlugInAsync(
            int id,
            CancellationToken cancellationToken)
        {
            var plugIn = await GetPlugInByIdFromContextAsync(
                id,
                cancellationToken);
            
            if(plugIn != null)
            {
                _logger.LogInformation("Starting PlugIn [{id}] [{name}]",
                    plugIn.Id,
                    plugIn.Name);
                
                await _manager.StartPlugInAsync(
                    plugIn,
                    cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task StopPlugInAsync(
            int id,
            CancellationToken cancellationToken)
        {
            var plugIn = await GetPlugInByIdFromContextAsync(
                id,
                cancellationToken);
            
            if(plugIn != null)
            {
                _logger.LogInformation("Stopping PlugIn [{id}] [{name}]",
                    plugIn.Id,
                    plugIn.Name);

                await _manager.StopPlugInAsync(
                    plugIn,
                    cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<ApiPlugIn?> UpdatePlugInAsync(
            int id,
            string name,
            string? description,
            CancellationToken cancellationToken)
        {
            return await UpdatePlugInAsync(
                id,
                (plugIn) =>
                {
                    if (plugIn != null)
                    {
                        plugIn.Description = description;
                        plugIn.Name = name;
                    }
                },
                cancellationToken);
        }

        /// <inheritdoc/>
        private async Task<ApiPlugIn?> UpdatePlugInAsync(
            int id, 
            Action<ModelPlugIn?> modify,
            CancellationToken cancellationToken)
        {
            var plugIn = await _context
                            .PlugIns
                            .FirstOrDefaultAsync(
                                _ => _.Id == id,
                                cancellationToken);

            modify(plugIn);

            return plugIn?.ToApiModel();
        }
    }
}