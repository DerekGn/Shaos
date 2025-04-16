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

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shaos.Repository;
using Shaos.Repository.Extensions;
using Shaos.Repository.Models;
using Shaos.Services.Exceptions;
using System.Linq.Expressions;

namespace Shaos.Services.Repositories
{
    public class PlugInInstanceRepository : BaseRepository, IPlugInInstanceRepository
    {
        private readonly ILogger<PlugInInstanceRepository> _logger;

        public PlugInInstanceRepository(
            ILogger<PlugInInstanceRepository> logger,
            ShaosDbContext context) : base(context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<int> CreateAsync(
            PlugIn plugIn,
            PlugInInstance plugInInstance,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(plugIn);
            ArgumentNullException.ThrowIfNull(plugInInstance);

            plugInInstance.PlugIn = plugIn;
            plugInInstance.PlugInId = plugIn.Id;

            plugIn.Instances.Add(plugInInstance);

            return await HandleDuplicatePlugInInstanceNameAsync(plugInInstance.Name, async () =>
            {
                await Context.SaveChangesAsync(cancellationToken);

                return plugInInstance.Id;
            });
        }

        /// <inheritdoc/>
        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return Context.Set<PlugInInstance>().DeleteAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await Context.Set<PlugInInstance>().AnyAsync(_ => _.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<PlugInInstance> GetAsync(
            Expression<Func<PlugInInstance, bool>>? filter = null,
            Func<IQueryable<PlugInInstance>, IOrderedQueryable<PlugInInstance>>? orderBy = null,
            bool withNoTracking = true,
            List<string>? includeProperties = null,
            CancellationToken cancellationToken = default)
        {
            return Context
                .Set<PlugInInstance>()
                .GetAsync(withNoTracking, filter, orderBy, includeProperties, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<PlugInInstance?> GetByIdAsync(
            int id,
            bool withNoTracking = true,
            List<string>? includeProperties = null,
            CancellationToken cancellationToken = default)
        {
            return Context
                .Set<PlugInInstance>()
                .GetByIdAsync(id, withNoTracking, includeProperties, cancellationToken);
        }

        /// <inheritdoc/>
        public IQueryable<PlugInInstance> GetQueryable(
            Expression<Func<PlugInInstance, bool>>? filter = null,
            Func<IQueryable<PlugInInstance>, IOrderedQueryable<PlugInInstance>>? orderBy = null,
            bool withNoTracking = true,
            List<string>? includeProperties = null)
        {
            return Context
                .Set<PlugInInstance>()
                .GetQueryable(withNoTracking, filter, orderBy, includeProperties);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(
            int id,
            bool? enabled = default,
            string? name = default,
            string? description = default,
            CancellationToken cancellationToken = default)
        {
            if(enabled.HasValue && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(description))
            {
                var plugInInstance = await Context.PlugInInstances.FirstOrDefaultAsync(_ => _.Id == id, cancellationToken) ?? throw new PlugInInstanceNotFoundException(id);

                await HandleDuplicatePlugInInstanceNameAsync(name, async () =>
                {
                    if (enabled.HasValue)
                    {
                        plugInInstance.Enabled = enabled.Value;
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        plugInInstance.Name = name;
                    }
                    
                    if (!string.IsNullOrEmpty(description))
                    {
                        plugInInstance.Description = description;
                    }

                    await Context.SaveChangesAsync(cancellationToken);

                    return plugInInstance;
                });
            }
        }

        private async Task<T> HandleDuplicatePlugInInstanceNameAsync<T>(string name, Func<Task<T>> operation)
        {
            try
            {
                return await operation();
            }
            catch (DbUpdateException exception) when (exception.InnerException is SqliteException sqliteException)
            {
                if (sqliteException.SqliteErrorCode == 0x13)
                {
                    _logger.LogWarning(exception, "Duplicate PlugIn Name: [{Name}] exists", name);

                    throw new PlugInInstanceNameExistsException(name);
                }

                throw;
            }
        }
    }
}