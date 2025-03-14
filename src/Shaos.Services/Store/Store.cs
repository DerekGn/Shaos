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
using Shaos.Repository.Models;
using System.Runtime.CompilerServices;

namespace Shaos.Services.Store
{
    /// <summary>
    /// The implementation of the <see cref="IStore"/>
    /// </summary>
    public class Store : IStore
    {
        private readonly ShaosDbContext _context;
        private readonly ILogger<Store> _logger;

        public Store(ILogger<Store> logger, ShaosDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInAsync(
            string name,
            string? description,
            CancellationToken cancellationToken = default)
        {
            var plugIn = new PlugIn()
            {
                Name = name,
                Description = description
            };

            await _context.PlugIns.AddAsync(plugIn, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("PlugIn [{Id}] [{Name}] Created", plugIn.Id, plugIn.Name);

            return plugIn.Id;
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInInstanceAsync(
            string name,
            string description,
            PlugIn plugIn,
            CancellationToken cancellationToken = default)
        {
            var plugInInstance = new PlugInInstance()
            {
                Description = description,
                Name = name,
                PlugIn = plugIn,
                PlugInId = plugIn.Id
            };

            plugIn.Instances.Add(plugInInstance);

            await _context.SaveChangesAsync(cancellationToken);

            return plugInInstance.Id;
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInNuGetPackageAsync(
            string name,
            string fileName,
            string version,
            PlugIn plugIn,
            CancellationToken cancellationToken = default)
        {
            var nuGetPackage = new NuGetPackage()
            {
                Name = name,
                FileName = fileName,
                Version = version.ToString(),
                PlugIn = plugIn,
                PlugInId = plugIn.Id
            };

            plugIn.NuGetPackage = nuGetPackage;

            await _context.SaveChangesAsync(cancellationToken);

            return nuGetPackage.Id;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync<T>(
            int id,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            // this is EF Core 7 enhancement performs select and delete in one operation
            await _context
                .Set<T>()
                .Where(_ => _.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PlugIn?> GetPlugInByIdAsync(
            int id,
            bool withNoTracking = true,
            CancellationToken cancellationToken = default)
        {
            var query = _context
                .PlugIns
                .Include(_ => _.NuGetPackage)
                .Include(_ => _.Instances)
                .AsQueryable();

            if (withNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(_ => _.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PlugIn?> GetPlugInByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            var plugin = await _context
                .PlugIns
                .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.Name == name, cancellationToken);

            return plugin;
        }

        /// <inheritdoc/>
        public async Task<PlugInInstance?> GetPlugInInstanceByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var plugInInstance = await _context
               .PlugInInstances
               //.Include(_ => _.PlugIn)
               //.Include(_ => _.PlugIn.NuGetPackage)
               .AsNoTracking()
               .FirstOrDefaultAsync(_ => _.Id == id,
               cancellationToken);

            return plugInInstance;
        }

        /// <inheritdoc/>
        public async Task<PlugInInstance?> GetPlugInInstanceByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            var plugInInstance = await _context
                .PlugInInstances
                .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.Name == name, cancellationToken);

            return plugInInstance;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<PlugIn> GetPlugInsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in _context.PlugIns
                .Include(_ => _.NuGetPackage)
                .Include(_ => _.Instances)
                .AsNoTracking()
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken))
            {
                yield return item;
            }
        }

        /// <inheritdoc/>
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PlugIn?> UpdatePlugInAsync(
            int id,
            string name,
            string? description,
            CancellationToken cancellationToken = default)
        {
            PlugIn? result = null;

            if(!await _context.PlugIns.AnyAsync(_ => _.Name == name && _.Id != id, cancellationToken))
            {
                var plugIn = await _context.PlugIns.FirstAsync(_ => _.Id == id, cancellationToken);

                plugIn.Name = name;
                plugIn.Description = description;

                await _context.SaveChangesAsync(cancellationToken);

                result = plugIn;
            }

            return result;
        }
    }
}