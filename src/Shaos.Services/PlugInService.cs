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
using Shaos.Repository;
using Shaos.Services.Extensions;

using ApiPlugIn = Shaos.Api.Model.v1.PlugIn;
using ModelPlugIn = Shaos.Repository.Models.PlugIn;

namespace Shaos.Services
{
    public class PlugInService : IPlugInService
    {
        private readonly ShaosDbContext _context;
        private readonly ILogger<PlugInService> _logger;

        public PlugInService(ILogger<PlugInService> logger, ShaosDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInAsync(string name, string? description, string code)
        {
            var plugin = new ModelPlugIn()
            {
                Code = code,
                Name = name,
                Description = description,
                IsEnabled = false
            };

            await _context.PlugIns.AddAsync(plugin);

            await _context.SaveChangesAsync();

            return plugin.Id;
        }

        /// <inheritdoc/>
        public async Task<ApiPlugIn?> GetPlugInByIdAsync(int id, CancellationToken cancellationToken)
        {
#warning map plugin state
            var plugin = await _context
                .PlugIns
            .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.Id == id, cancellationToken);

            return plugin?.ToApiModel();
        }

        /// <inheritdoc/>
        public async Task<ApiPlugIn?> GetPlugInByNameAsync(string name, CancellationToken cancellationToken)
        {
            var plugin = await _context
                .PlugIns
                .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.Name == name, cancellationToken);

            return plugin?.ToApiModel();
        }

        public async IAsyncEnumerable<ApiPlugIn> GetPlugInsAsync()
        {
#warning map state
            await foreach (var item in _context.PlugIns.AsNoTracking().AsAsyncEnumerable())
            {
                yield return item.ToApiModel();
            }
        }

        /// <inheritdoc/>
        public async Task<bool> PlugInWithNameExistsAsync(string name, CancellationToken cancellationToken)
        {
            return await _context.PlugIns.AnyAsync(_ => _.Name == name, cancellationToken: cancellationToken);
        }
    }
}