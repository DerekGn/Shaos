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

// Ignore Spelling: Nuget

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using NuGet.Versioning;
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Services.IO;
using System.Runtime.CompilerServices;

namespace Shaos.Services
{
    public class PlugInService : BasePlugInService, IPlugInService
    {
        private readonly IFileStoreService _fileStoreService;

        public PlugInService(
            ILogger<PlugInService> logger,
            IFileStoreService fileStoreService,
            IDbContext context) : base(logger, context)
        {
            _fileStoreService = fileStoreService ?? throw new ArgumentNullException(nameof(fileStoreService));
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInAsync(
            CreatePlugIn createPlugIn,
            CancellationToken cancellationToken = default)
        {
            var plugIn = new PlugIn()
            {
                Name = createPlugIn.Name,
                Description = createPlugIn.Description
            };

            await Context.PlugIns.AddAsync(plugIn, cancellationToken);

            await Context.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("PlugIn [{Id}] [{Name}] Created", plugIn.Id, plugIn.Name);

            return plugIn.Id;
        }

        /// <inheritdoc/>
        public async Task<int> CreatePlugInInstanceAsync(
            int id,
            CreatePlugInInstance create,
            CancellationToken cancellationToken = default)
        {
            int result = 0;

            var plugIn = await GetPlugInByIdFromContextAsync(
                id,
                false,
                cancellationToken: cancellationToken);

            if (plugIn != null)
            {
                Logger.LogInformation("Creating PlugInInstance. PlugIn: [{Id}]",
                   id);

                var plugInInstance = new PlugInInstance()
                {
                    Description = create.Description,
                    Enabled = false,
                    Name = create.Name,
                    PlugIn = plugIn,
                    PlugInId = plugIn.Id
                };

                plugIn.Instances.Add(plugInInstance);
                await Context.SaveChangesAsync(cancellationToken);

                result = plugInInstance.Id;
            }
            else
            {
                Logger.LogWarning("PlugIn: [{Id}] Not Found", id);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task UpdatePlugInNuGetPackageAsync(
            int id,
            string fileName,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            var plugIn = await GetPlugInByIdFromContextAsync(id, false, cancellationToken);

            if (plugIn != null)
            {
                Logger.LogDebug("Storing NuGet Package. PlugIn: [{Id}] FileName: [{FileName}]",
                    id,
                    fileName);

                var filePath = await _fileStoreService.WritePlugInNuGetPackageFileStreamAsync(
                    plugIn.Id,
                    fileName,
                    stream,
                    cancellationToken);

                var version = GetNuGetPackageVersion(filePath);

                if (plugIn.NuGetPackage == null)
                {
                    plugIn.NuGetPackage = new NuGetPackage()
                    {
                        FileName = fileName,
                        Version = version.ToString(),
                    };
                }
                else
                {
                    plugIn.NuGetPackage.FileName = fileName;
                    plugIn.NuGetPackage.Version = version.ToString();
                }

                await Context.SaveChangesAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task DeletePlugInAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
#warning Check if plugin used and delete plugin package
            Logger.LogInformation("PlugIn [{Id}] Deleting", id);

            // Delete code and compiled assembly files
            _fileStoreService.DeletePlugInPackageFolder(id);

            // this is EF COre 7 enhancement performs select and delete in one operation
            await Context.PlugIns.Where(_ => _.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeletePlugInInstanceAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("PlugInInstance [{Id}] Deleting", id);

            // this is EF COre 7 enhancement performs select and delete in one operation
            await Context.PlugInInstances.Where(_ => _.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PlugIn?> GetPlugInByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            PlugIn? plugin = await GetPlugInByIdFromContextAsync(
                id,
                cancellationToken: cancellationToken);

            return plugin;
        }

        /// <inheritdoc/>
        public async Task<PlugIn?> GetPlugInByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            var plugin = await Context
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
            var plugInInstance = await Context
                .PlugInInstances
                .Include(_ => _.PlugIn)
                .Include(_ => _.PlugIn.NuGetPackage)
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
            var plugInInstance = await Context
                .PlugInInstances
                .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.Name == name, cancellationToken);

            return plugInInstance;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<PlugIn> GetPlugInsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in Context.PlugIns
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
        public async Task SetPlugInInstanceEnableAsync(
            int id,
            bool enable,
            CancellationToken cancellationToken = default)
        {
            await UpdatePlugInInstanceAsync(id, (plugInInstance) =>
            {
                if (plugInInstance != null)
                {
                    plugInInstance.Enabled = enable;
                }
            },
            cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PlugIn?> UpdatePlugInAsync(
            int id,
            UpdatePlugIn update,
            CancellationToken cancellationToken = default)
        {
            return await UpdatePlugInAsync(
                id,
                (plugIn) =>
                {
                    if (plugIn != null)
                    {
                        plugIn.Description = update.Description;
                        plugIn.Name = update.Name;
                    }
                },
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UpdatePlugInInstanceAsync(
            int id,
            UpdatePlugInInstance update,
            CancellationToken cancellationToken = default)
        {
            await UpdatePlugInInstanceAsync(id, (plugInInstance) =>
            {
                if (plugInInstance != null)
                {
                    plugInInstance.Description = update.Description;
                    plugInInstance.Name = update.Name;
                }
            },
            cancellationToken);
        }

        private static NuGetVersion GetNuGetPackageVersion(string? filePath)
        {
            using FileStream inputStream = new FileStream(filePath, FileMode.Open);
            using PackageArchiveReader reader = new PackageArchiveReader(inputStream);
            NuspecReader nuspec = reader.NuspecReader;

            return nuspec.GetVersion();
        }

        /// <inheritdoc/>
        private async Task<PlugIn?> UpdatePlugInAsync(
            int id,
            Action<PlugIn?> modify,
            CancellationToken cancellationToken = default)
        {
            var plugIn = await Context
                .PlugIns
                .FirstOrDefaultAsync(_ => _.Id == id, cancellationToken);

            modify(plugIn);

            await Context.SaveChangesAsync(cancellationToken);

            return plugIn;
        }

        /// <inheritdoc/>
        private async Task<PlugInInstance?> UpdatePlugInInstanceAsync(
            int id,
            Action<PlugInInstance?> modify,
            CancellationToken cancellationToken = default)
        {
            var plugInInstance = await Context
                .PlugInInstances
                .FirstOrDefaultAsync(_ => _.Id == id, cancellationToken);

            modify(plugInInstance);

            await Context.SaveChangesAsync(cancellationToken);

            return plugInInstance;
        }
    }
}