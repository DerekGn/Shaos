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
using Serilog.Events;
using Shaos.Repository.Exceptions;
using Shaos.Repository.Extensions;
using Shaos.Repository.Models;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Shaos.Repository
{
    /// <summary>
    /// The repository implementation
    /// </summary>
    public class PlugInRepository : IPlugInRepository
    {
        private readonly ShaosDbContext _context;
        private readonly ILogger<PlugInRepository> _logger;

        /// <summary>
        /// Create an instance of the <see cref="PlugInRepository"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{T}"/> instance</param>
        /// <param name="context">The <see cref="ShaosDbContext"/></param>
        public PlugInRepository(ILogger<PlugInRepository> logger,
                               ShaosDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <inheritdoc/>
        public async Task AddAsync<T>(T entity,
                                      CancellationToken cancellationToken = default) where T : BaseEntity
        {
            await _context
                .Set<T>()
                .AddAsync(entity, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> AnyAsync<T>(Expression<Func<T, bool>>? predicate,
                                            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return await _context
                .Set<T>()
                .AnyAsync(predicate, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<int> CreatePackageAsync(PlugIn plugIn,
                                                  Package package,
                                                  CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(plugIn);
            ArgumentNullException.ThrowIfNull(package);

            _logger.LogDebug("Creating new package for PlugIn: [{Id}] Package: [{Package}]",
                plugIn.Id,
                package);

            plugIn.Package = package;

            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInAsync(PlugIn plugIn,
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
        public async Task<int> CreatePlugInInstanceAsync(PlugIn plugIn,
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
        public Task DeleteAsync<T>(int id,
                                   CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return _context.Set<T>().DeleteAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<T?> GetByIdAsync<T>(int id,
                                        bool withNoTracking = true,
                                        List<string>? includeProperties = null,
                                        CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return _context
                .Set<T>()
                .GetByIdAsync(id,
                              withNoTracking,
                              includeProperties,
                              cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<LogLevelSwitch?> GetByNameAsync(string name,
                                                          CancellationToken cancellationToken = default)
        {
            return await _context
                .LogLevelSwitches
                .Where(_ => _.Name == name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<T> GetEnumerableAsync<T>(Expression<Func<T, bool>>? predicate = null,
                                                         Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                                         bool withNoTracking = true,
                                                         List<string>? includeProperties = null,
                                                         CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return _context
                .Set<T>()
                .GetAsync(withNoTracking,
                             predicate,
                             orderBy,
                             includeProperties,
                             cancellationToken);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<T> GetEnumerableAsync<T>([EnumeratorCancellation] CancellationToken cancellationToken = default) where T : BaseEntity
        {
            await foreach (var item in _context
                .Set<T>()
                .AsNoTracking()
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken))
            {
                yield return item;
            }
        }

        /// <inheritdoc/>
        public Task<T?> GetFirstOrDefaultAsync<T>(Expression<Func<T, bool>>? predicate = null,
                                                  CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var query = _context.Set<T>();

            if (predicate != null)
            {
                return query.FirstOrDefaultAsync(predicate, cancellationToken);
            }
            else
            {
                return query.FirstOrDefaultAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public IQueryable<T> GetQueryable<T>(Expression<Func<T, bool>>? predicate = null,
                                             Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                             bool withNoTracking = true,
                                             List<string>? includeProperties = null) where T : BaseEntity
        {
            return _context
                .Set<T>()
                .GetQueryable(withNoTracking, predicate, orderBy, includeProperties);
        }

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PlugIn?> UpdatePlugInAsync(int id,
                                                     string name,
                                                     string description,
                                                     CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);

            var plugIn = await _context.PlugIns.FirstAsync(_ => _.Id == id, cancellationToken) ?? throw new NotFoundException(id);

            return await HandleDuplicatePlugInNameAsync(name, async () =>
            {
                plugIn.Name = name;
                plugIn.Description = description;

                await _context.SaveChangesAsync(cancellationToken);

                return plugIn;
            });
        }

        /// <inheritdoc/>
        public async Task UpdatePlugInInstanceAsync(int id,
                                                    bool? enabled = null,
                                                    string? name = null,
                                                    string? description = null,
                                                    CancellationToken cancellationToken = default)
        {
            if (enabled.HasValue && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(description))
            {
                var plugInInstance = await _context.PlugInInstances.FirstOrDefaultAsync(_ => _.Id == id, cancellationToken) ?? throw new NotFoundException(id);

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

                    await _context.SaveChangesAsync(cancellationToken);

                    return plugInInstance;
                });
            }
        }

        /// <inheritdoc/>
        public async Task<LogLevelSwitch> UpsertLogLevelSwitchAsync(string name,
                                                                    LogEventLevel level,
                                                                    CancellationToken cancellationToken = default)
        {
            var logLevelSwitch = await _context
                .LogLevelSwitches
                .FirstOrDefaultAsync(_ => _.Name == name, cancellationToken);

            if (logLevelSwitch != null)
            {
                logLevelSwitch.Level = level;
            }
            else
            {
                await _context
                    .LogLevelSwitches
                    .AddAsync(new LogLevelSwitch()
                    {
                        Name = name,
                        Level = level
                    },
                    cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return logLevelSwitch!;
        }

        private async Task<T> HandleDuplicatePlugInInstanceNameAsync<T>(string name,
                                                                        Func<Task<T>> operation)
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

                    throw new NameExistsException(name);
                }

                throw;
            }
        }

        private async Task<T> HandleDuplicatePlugInNameAsync<T>(string name,
                                                                Func<Task<T>> operation)
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

                    throw new NameExistsException(name);
                }

                throw;
            }
        }
    }
}