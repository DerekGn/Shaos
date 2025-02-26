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

namespace Shaos.Services
{
    public class PlugInRuntime : IPlugInRuntime
    {
        private readonly ICompilerService _compilerService;
        private IAssemblyCache _assembleCache;
        private List<ExecutingPlugIn> _executingPlugIns;
        private ILogger<PlugInRuntime> _logger; 

        public PlugInRuntime(
            ILogger<PlugInRuntime> logger,
            IAssemblyCache assemblyCache,
            ICompilerService compilerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _assembleCache = assemblyCache ?? throw new ArgumentNullException(nameof(assemblyCache));
            _compilerService = compilerService ?? throw new ArgumentNullException(nameof(compilerService));
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
            CancellationToken cancellationToken)
        {
            var executingPlugIn = _executingPlugIns
                .FirstOrDefault(_ => _.Id == plugIn.Id);

            if(executingPlugIn == null)
            {
                _logger.LogInformation("Starting PlugIn: [{Id}] Name: [{Name}]",
                    plugIn.Id, plugIn.Name);

                var executingPlugin = new ExecutingPlugIn()
                {
                    Id = plugIn.Id,
                    State = ExecutionState.InActive
                };

                _executingPlugIns.Add(executingPlugin);

                var files = plugIn.CodeFiles.Select(_ => _.FilePath);

#warning TODO store assembly to specific folder
#warning TODO async compile
                using Stream stream = File.Create("f:\\Temp\\assembly.dll");
                var result = await _compilerService.CompileAsync("assembly.dll", stream, files, cancellationToken);

                executingPlugin.State = result.Success ? ExecutionState.Compiled : ExecutionState.CompileFailed;

                executingPlugin.CompileResults = result.Diagnostics.Select(_ => _.ToString());
            }
        }

        /// </inheritdoc>
        public async Task StopPlugInAsync(
            PlugIn plugIn,
            CancellationToken cancellationToken)
        {
            var executingPlugIn = _executingPlugIns
                .FirstOrDefault(_ => _.Id == plugIn.Id);

            if (executingPlugIn != null)
            {
                _logger.LogInformation("Stopping PlugIn: [{Id}] Name: [{Name}]",
                    plugIn.Id, plugIn.Name);
            }
        }
    }
}