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
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Services.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Shaos.Services.Store
{
    /// <summary>
    /// The implementation of the <see cref="IStore"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
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
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);

            var plugIn = new PlugIn()
            {
                Name = name,
                Description = description
            };

            return await HandleDuplicatePlugInNameAsync(name, async () =>
            {
                await _context.PlugIns.AddAsync(plugIn, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("PlugIn [{Id}] [{Name}] Created", plugIn.Id, plugIn.Name);

                return plugIn.Id;
            });
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInInstanceAsync(
            string name,
            string description,
            PlugIn plugIn,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(description);

            var plugInInstance = new PlugInInstance()
            {
                Description = description,
                Name = name,
                PlugIn = plugIn,
                PlugInId = plugIn.Id
            };

            plugIn.Instances.Add(plugInInstance);

            return await HandleDuplicatePlugInInstanceNameAsync(name, async () =>
            {
                await _context.SaveChangesAsync(cancellationToken);

                return plugInInstance.Id;
            });
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInPackageAsync(
            PlugIn plugIn,
            string fileName,
            string assemblyFile,
            string version,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(assemblyFile);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(version);

            var package = new Package()
            {
                FileName = fileName,
                AssemblyFile = assemblyFile,
                PlugIn = plugIn,
                PlugInId = plugIn.Id,
                Version = version
            };

            plugIn.Package = package;

            await _context.SaveChangesAsync(cancellationToken);

            return package.Id;
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
        public async IAsyncEnumerable<LogLevelSwitch> GetLogLevelSwitchesAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in _context.LogLevelSwitches
                .AsNoTracking()
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken))
            {
                yield return item;
            }
        }

        /// <inheritdoc/>
        public async Task<PlugIn?> GetPlugInByIdAsync(
            int id,
            bool withNoTracking = true,
            CancellationToken cancellationToken = default)
        {
            var query = _context
                .PlugIns
                .Include(_ => _.Package)
                .Include(_ => _.Instances)
                .AsQueryable();

            if (withNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(_ => _.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PlugInInstance?> GetPlugInInstanceByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var plugInInstance = await _context
               .PlugInInstances
               .Include(_ => _.PlugIn)
               .Include(_ => _.PlugIn!.Package)
               .AsNoTracking()
               .FirstOrDefaultAsync(_ => _.Id == id, cancellationToken);

            return plugInInstance;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<PlugIn> GetPlugInsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in _context.PlugIns
                .Include(_ => _.Package)
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
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);

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
        public async Task UpdatePlugInInstanceAsync(
            int id,
            string name,
            string? description,
            CancellationToken cancellationToken)
        {
            var plugInInstance = await _context.PlugInInstances.FirstOrDefaultAsync(_ => _.Id == id, cancellationToken) ?? throw new PlugInInstanceNotFoundException(id);

            await HandleDuplicatePlugInInstanceNameAsync(name, async () =>
            {
                plugInInstance.Name = name;
                plugInInstance.Description = description;

                await _context.SaveChangesAsync(cancellationToken);

                return plugInInstance;
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

        /// </<inheritdoc/>
        public async Task<LogLevelSwitch> UpsertLogLevelSwitchAsync(
            string name,
            LogEventLevel level,
            CancellationToken cancellationToken = default)
        {
            var logLevelSwitch = await _context.LogLevelSwitches.SingleAsync(_ => _.Name == name);

            if(logLevelSwitch != null)
            {
                logLevelSwitch.Level = level;
            }
            else
            {
                await _context.LogLevelSwitches.AddAsync(new LogLevelSwitch()
                {
                    Name = name,
                    Level = level
                });
            }

            await _context.SaveChangesAsync();

            return logLevelSwitch!;
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