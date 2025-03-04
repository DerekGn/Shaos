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

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Services.IO;
using Shaos.Services.Options;

namespace Shaos.Services.Compiler
{
    public class PlugInCompilerService : BasePlugInService, IPlugInCompilerService
    {
        private readonly ICompilerService _compilerService;
        private readonly IFileStoreService _fileStoreService;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<PlugInCompilerServiceOptions> _options;

        public PlugInCompilerService(
            ILogger<PlugInCompilerService> logger,
            ShaosDbContext context,
            IMemoryCache memoryCache,
            ICompilerService compilerService,
            IFileStoreService fileStoreService,
            IOptions<PlugInCompilerServiceOptions> options) : base(logger, context)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _compilerService = compilerService ?? throw new ArgumentNullException(nameof(compilerService));
            _fileStoreService = fileStoreService ?? throw new ArgumentNullException(nameof(fileStoreService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// </<inheritdoc/>
        public bool CompilationInProgress(int id)
        {
            return _memoryCache.TryGetValue<CompilationStatus>(id, out var _);
        }

        /// <inheritdoc/>
        public CompilationStatus? GetCompilationStatus(int id)
        {
            return _memoryCache.Get<CompilationStatus>(id);
        }

        /// </<inheritdoc/>
        public void StartCompilation(PlugIn plugIn)
        {
            if (plugIn != null)
            {
                var compilationStatus = _memoryCache.Get<CompilationStatus>(plugIn.Id);

                if (compilationStatus == null)
                {
                    compilationStatus = new CompilationStatus()
                    {
                        Id = plugIn.Id,
                        PlugIn = plugIn,
                    };

                    _memoryCache.Set(plugIn.Id, compilationStatus, _options.Value.CacheTime);
                }

                Logger.LogInformation("Starting PlugIn: [{Id}] Name: [{Name}] Compilation",
                        plugIn.Id,
                        plugIn.Name);

                if (!compilationStatus.Active)
                {
                    compilationStatus.StartTime = DateTime.UtcNow;
                    StartPlugInCompilationTask(compilationStatus);
                }
            }
        }

        private async Task<CompilationResult> CompilePlugInCodeFilesAsync(
            PlugIn plugIn,
            CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Compiling PlugIn: [{Id}] Name: [{Name}]",
                plugIn.Id,
                plugIn.Name);

            var assemblyFileName = Path.GetRandomFileName();

            using var assemblyFileStream = _fileStoreService
                .CreateAssemblyFileStream(
                    plugIn.Id.ToString(),
                    assemblyFileName,
                    out string? assemblyFilePath);

            var files = plugIn.CodeFiles.Select(_ => _.FilePath);

            var compileResult = new CompilationResult()
            {
                AssemblyFilePath = assemblyFilePath,
                Result = await _compilerService.CompileAsync(assemblyFileName, assemblyFileStream, files, cancellationToken)
            };

            return compileResult;
        }

        private Task StartPlugInCompilationTask(CompilationStatus compilationStatus)
        {
            compilationStatus.CancellationTokenSource = new CancellationTokenSource();

            var plugIn = compilationStatus.PlugIn!;
            var cancellationToken = compilationStatus.CancellationTokenSource.Token;

            return Task
                .Run(async () => await CompilePlugInCodeFilesAsync(plugIn, cancellationToken),
                    cancellationToken)
                .ContinueWith(async (_) =>
                {
                    if (_ != null && _.IsCompletedSuccessfully)
                    {
                        compilationStatus.EndTime = DateTime.UtcNow;
                        compilationStatus.Result = _.Result;

                        await UpdatePlugInCompilationResultAsync(
                            plugIn.Id,
                            _.Result!,
                            cancellationToken);
                    }
                    else
                    {
                        Logger.LogError(_.Exception, "Exception occurred compiling PlugIn: [{Id}] Name: [{Name}]",
                            plugIn.Id,
                            plugIn.Name);
                    }
                },
                cancellationToken);
        }

        private async Task UpdatePlugInCompilationResultAsync(
            int id,
            CompilationResult result,
            CancellationToken cancellationToken)
        {
            var plugIn = await GetPlugInByIdFromContextAsync(id, false, cancellationToken);

            if (plugIn != null)
            {
                Logger.LogInformation("Updating compilation result for PlugIn: [{Id}] Assembly File Path: [{FilePath}]",
                    id, result.AssemblyFilePath);

                plugIn.AssemblyFilePath = result.AssemblyFilePath;

                await Context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                Logger.LogWarning("Unable to find PlugIn: [{Id}]", id);
            }
        }
    }
}