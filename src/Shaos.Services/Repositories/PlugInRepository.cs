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
    public class PlugInRepository : IPlugInRepository
    {
        private readonly ShaosDbContext _context;
        private readonly ILogger<PlugInRepository> _logger;

        public PlugInRepository(
            ILogger<PlugInRepository> logger,
            ShaosDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInAsync(
            PlugIn plugIn,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(plugIn);

            return await HandleDuplicatePlugInNameAsync(plugIn.Name, async () =>
            {
                await _context.PlugIns.AddAsync(plugIn, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("PlugIn [{Id}] [{Name}] Created", plugIn.Id, plugIn.Name);

                return plugIn.Id;
            });
        }

        /// <inheritdoc/>
        public Task<int> DeletePlugInAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Set<PlugIn>().DeleteAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<PlugIn> GetAsync(
            Expression<Func<PlugIn, bool>>? filter = null,
            Func<IQueryable<PlugIn>, IOrderedQueryable<PlugIn>>? orderBy = null,
            bool withNoTracking = true,
            List<string>? includeProperties = null,
            CancellationToken cancellationToken = default)
        {
            return _context
                .Set<PlugIn>()
                .GetAsync(withNoTracking, filter, orderBy, includeProperties, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PlugIn?> UpdatePlugInAsync(
            int id,
            string name,
            string description,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(description);

            var plugIn = await _context.PlugIns.FirstAsync(_ => _.Id == id, cancellationToken) ?? throw new PlugInNotFoundException(id);

            return await HandleDuplicatePlugInNameAsync(name, async () =>
            {
                plugIn.Name = name;
                plugIn.Description = description;

                await _context.SaveChangesAsync(cancellationToken);

                return plugIn;
            });
        }

        /// <inheritdoc/>
        public async Task UpdatePlugInPackageAsync(
            PlugIn plugIn,
            string fileName,
            string assemblyFile,
            string version,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(assemblyFile);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(version);

            if (plugIn.Package != null)
            {
                plugIn.Package.FileName = fileName;
                plugIn.Package.AssemblyFile = assemblyFile;
                plugIn.Package.Version = version;

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task<T> HandleDuplicatePlugInNameAsync<T>(string name, Func<Task<T>> operation)
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

                    throw new PlugInNameExistsException(name);
                }

                throw;
            }
        }
    }
}