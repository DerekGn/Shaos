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

namespace Shaos.Services
{
    public class PlugInService : IPlugInService
    {
        private readonly ShaosDbContext _context;
        private readonly IFileStoreService _fileStoreService;
        private readonly ILogger<PlugInService> _logger;
        private readonly IPlugInRuntime _manager;

        public PlugInService(
            ILogger<PlugInService> logger,
            IPlugInRuntime manager,
            IFileStoreService fileStoreService,
            ShaosDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _fileStoreService = fileStoreService ?? throw new ArgumentNullException(nameof(fileStoreService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInAsync(
            string name,
            string? description,
            CancellationToken cancellationToken)
        {
            var plugIn = new PlugIn()
            {
                Name = name,
                Description = description,
                IsEnabled = false
            };

            await _context.PlugIns.AddAsync(plugIn);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("PlugIn [{Id}] [{Name}] Created", plugIn.Id, plugIn.Name);

            return plugIn.Id;
        }

        /// <inheritdoc/>
        public async Task DeletePlugInAsync(
            int id,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("PlugIn [{Id}] Deleting", id);

            _fileStoreService.DeleteFolder(id.ToString());

            // this is EF COre 7 enhancement performs select and delete in one operation
            await _context.PlugIns.Where(_ => _.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeletePlugInCodeFileAsync(
            int id,
            int codeFileId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("PlugIn [{Id}] CodeFile [{CodeFileId}] Deleting", id, codeFileId);

            var plugIn = await GetPlugInByIdFromContextAsync(id, false, cancellationToken);

            if (plugIn != null)
            {
                var codeFile = plugIn.CodeFiles.FirstOrDefault(_ => _.Id == codeFileId);

                if (codeFile != null)
                {
                    plugIn.CodeFiles.Remove(codeFile);
                    _context.Remove(codeFile);

                    _fileStoreService.DeleteFile(codeFile.FilePath);

                    await _context.SaveChangesAsync();
                }
            }
        }

        /// <inheritdoc/>
        public async Task<PlugIn?> GetPlugInByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            PlugIn? plugin = await GetPlugInByIdFromContextAsync(
                id,
                cancellationToken: cancellationToken);

            return plugin;
        }

        /// <inheritdoc/>
        public async Task<PlugIn?> GetPlugInByNameAsync(
            string name,
            CancellationToken cancellationToken)
        {
            var plugin = await _context
                .PlugIns
                .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.Name == name, cancellationToken);

            return plugin;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<PlugIn> GetPlugInsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in _context.PlugIns
                .Include(_ => _.CodeFiles)
                .AsNoTracking()
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken))
            {
                yield return item;
            }
        }

        /// <inheritdoc/>
        public async Task SetPlugInEnabledStateAsync(
            int id,
            bool isEnabled,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Setting PlugIn [{Id}] State To [{IsEnabled}]", id, isEnabled);

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
        public async Task<PlugIn?> UpdatePlugInAsync(
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
        public async Task UploadPlugInCodeFileAsync(
            int id,
            string fileName,
            Stream stream,
            CancellationToken cancellationToken)
        {
            var plugIn = await GetPlugInByIdFromContextAsync(
                id,
                false,
                cancellationToken: cancellationToken);

            if (plugIn != null)
            {
                var filePath = await _fileStoreService.WriteFileStreamAsync(
                    plugIn.Id.ToString(),
                    fileName,
                    stream,
                    cancellationToken);

                if (!plugIn.CodeFiles.Any(_ => string.Compare(_.FileName, fileName, true) == 0))
                {
                    plugIn.CodeFiles.Add(new CodeFile()
                    {
                        FileName = fileName,
                        FilePath = filePath!
                    });

                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        private async Task<PlugIn?> GetPlugInByIdFromContextAsync(
            int id,
            bool withNoTracking = true,
            CancellationToken cancellationToken = default)
        {
            var query = _context.PlugIns.Include(_ => _.CodeFiles).AsQueryable();

            if (withNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(_ => _.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        private async Task<PlugIn?> UpdatePlugInAsync(
            int id,
            Action<PlugIn?> modify,
            CancellationToken cancellationToken)
        {
            var plugIn = await _context
                            .PlugIns
                            .FirstOrDefaultAsync(
                                _ => _.Id == id,
                                cancellationToken);

            modify(plugIn);

            return plugIn;
        }
    }
}