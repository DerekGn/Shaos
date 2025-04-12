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
    public class PlugInInstanceRepository : IPlugInInstanceRepository
    {
        private readonly ShaosDbContext _context;
        private readonly ILogger<PlugInInstanceRepository> _logger;

        public PlugInInstanceRepository(
            ILogger<PlugInInstanceRepository> logger,
            ShaosDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInInstanceAsync(
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
                await _context.SaveChangesAsync(cancellationToken);

                return plugInInstance.Id;
            });
        }

        /// <inheritdoc/>
        public Task DeletePlugInInstanceAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Set<PlugInInstance>().DeleteAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<PlugInInstance> GetAsync(
            Expression<Func<PlugInInstance, bool>>? filter = null,
            Func<IQueryable<PlugInInstance>, IOrderedQueryable<PlugInInstance>>? orderBy = null,
            bool withNoTracking = true,
            List<string>? includeProperties = null,
            CancellationToken cancellationToken = default)
        {
            return _context
                .Set<PlugInInstance>()
                .GetAsync(withNoTracking, filter, orderBy, includeProperties, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UpdatePlugInInstanceAsync(
            int id,
            string name,
            string description,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(description);

            var plugInInstance = await _context.PlugInInstances.FirstOrDefaultAsync(_ => _.Id == id, cancellationToken) ?? throw new PlugInInstanceNotFoundException(id);

            await HandleDuplicatePlugInInstanceNameAsync(name, async () =>
            {
                plugInInstance.Name = name;
                plugInInstance.Description = description;

                await _context.SaveChangesAsync(cancellationToken);

                return plugInInstance;
            });
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