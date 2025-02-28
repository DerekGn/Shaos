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

using Microsoft.Extensions.Logging;
using Shaos.Repository.Models;

#warning Limit number of executing plugins
#warning Support multi-instancing of plugins

namespace Shaos.Services.Runtime
{
    public class PlugInRuntime : IPlugInRuntime
    {
        private readonly List<ExecutingPlugIn> _executingPlugIns;
        private readonly IFileStoreService _fileStoreService;
        private readonly ILogger<PlugInRuntime> _logger;

        public PlugInRuntime(
            ILogger<PlugInRuntime> logger,
            IFileStoreService fileStoreService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileStoreService = fileStoreService ?? throw new ArgumentNullException(nameof(fileStoreService));

            _executingPlugIns = new List<ExecutingPlugIn>();
        }

        /// </inheritdoc>
        public ExecutingPlugIn? GetExecutingPlugIn(int id)
        {
            return _executingPlugIns.FirstOrDefault(_ => _.Id == id);
        }

        /// <inheritdoc/>
        public IEnumerable<ExecutingPlugIn> GetExecutingPlugIns()
        {
            foreach (var executingPlugIn in _executingPlugIns)
            {
                yield return executingPlugIn;
            }
        }

        /// </inheritdoc>
        public async Task StartPlugInAsync(
            PlugIn plugIn,
            CancellationToken cancellationToken = default)
        {
            var executingPlugIn = _executingPlugIns
                .FirstOrDefault(_ => _.Id == plugIn.Id);

            if (executingPlugIn == null)
            {
                _logger.LogInformation("Creating Executing PlugIn: [{Id}] Name: [{Name}]",
                    plugIn.Id, plugIn.Name);

                executingPlugIn = new ExecutingPlugIn()
                {
                    Id = plugIn.Id,
                    State = ExecutionState.InActive
                };

                _executingPlugIns.Add(executingPlugIn);
            }

            if (executingPlugIn.State == ExecutionState.InActive)
            {
                _ = Task
                    .Run(async () => await CompilePlugInCodeAsync(plugIn, executingPlugIn, cancellationToken), cancellationToken)
                    .ContinueWith(async (_) =>
                    {
                        if (TaskComplete(nameof(CompilePlugInCodeAsync), _))
                        {
                            await ExecutePlugInAsync(plugIn, executingPlugIn, cancellationToken);
                        }
                    },
                    cancellationToken);
            }
            else
            {
                _logger.LogWarning("PlugIn: [{Id}] Name: [{Name}] Already Started", plugIn.Id, plugIn.Name);
            }
        }

        /// </inheritdoc>
        public async Task StopPlugInAsync(
            PlugIn plugIn,
            CancellationToken cancellationToken = default)
        {
            var executingPlugIn = _executingPlugIns
                .FirstOrDefault(_ => _.Id == plugIn.Id);

            if (executingPlugIn != null)
            {
                _ = Task
                    .Run(async () => await StopExecutingPlugInAsync(plugIn, executingPlugIn, cancellationToken), cancellationToken);
            }
            else
            {
                _logger.LogInformation("Executing PlugIn: [{Id}] Name: [{Name}] Not Found",
                    plugIn.Id, plugIn.Name);
            }
        }

        private async Task CompilePlugInCodeAsync(
            PlugIn plugIn,
            ExecutingPlugIn executingPlugIn,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Compiling PlugIn: [{Id}] Name: [{Name}]",
                plugIn.Id,
                plugIn.Name);

            var assemblyFileName = Path.GetRandomFileName();

            using var assemblyFileStream = _fileStoreService
                .CreateAssemblyFileStream(
                    plugIn.Id.ToString(),
                    assemblyFileName,
                    out string? assemblyFilePath);

#warning MOVE
            //executingPlugIn.AssemblyFilePath = assemblyFilePath;

            //var files = plugIn.CodeFiles.Select(_ => _.FilePath);

            //var result = await _compilerService.CompileAsync(assemblyFileName, assemblyFileStream, files, cancellationToken);

            //executingPlugIn.State = result.Success ? ExecutionState.Compiled : ExecutionState.CompileFailed;
            //executingPlugIn.CompileResults = result.Diagnostics.Select(_ => _.ToString());
        }

        private async Task ExecutePlugInAsync(
            PlugIn plugIn,
            ExecutingPlugIn executingPlugIn,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Executing PlugIn: [{Id}] Name: [{Name}]",
                plugIn.Id,
                plugIn.Name);
        }

        private async Task StopExecutingPlugInAsync(
            PlugIn plugIn,
            ExecutingPlugIn executingPlugIn,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Executing PlugIn: [{Id}] Name: [{Name}]",
                    plugIn.Id, plugIn.Name);
        }

        private bool TaskComplete(string name, Task task)
        {
            if (task != null && !task.IsCompletedSuccessfully)
            {
                _logger.LogError(task.Exception, "Exception occurred in Task [{Name}]", name);
            }

            return task != null && task.IsCompletedSuccessfully;
        }
    }
}