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
    public class PlugInManager : IPlugInManager
    {
        private List<ActivatedPlugIn> _activatedPlugIns;
        private IAssemblyCache _assembleCache;
        private ILogger<PlugInManager> _logger; 

        public PlugInManager(ILogger<PlugInManager> logger, IAssemblyCache assemblyCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _assembleCache = assemblyCache ?? throw new ArgumentNullException(nameof(assemblyCache));
            _activatedPlugIns = new List<ActivatedPlugIn>();
        }

        /// </inheritdoc>
        public async Task StartPlugInAsync(
            PlugIn plugIn,
            CancellationToken cancellationToken)
        {
            var activatedPlugIn = _activatedPlugIns.FirstOrDefault();

            if(activatedPlugIn == null)
            {
                await ActivatePlugInAsync(plugIn);                
            }
            else
            {
                await ActivatePlugInAsync(activatedPlugIn);
            }            
        }

        private async Task ActivatePlugInAsync(PlugIn plugIn)
        {
            _logger.LogInformation("Activating PlugIn [{id}] [{name}]", plugIn.Id, plugIn.Name);

            
            throw new NotImplementedException();
        }

        private async Task ActivatePlugInAsync(ActivatedPlugIn activatedPlugIn)
        {
            throw new NotImplementedException();
        }

        /// </inheritdoc>
        public async Task StopPlugInAsync(
            PlugIn plugIn,
            CancellationToken cancellationToken)
        {
            var activatedPlugIn = _activatedPlugIns.FirstOrDefault();

            if(activatedPlugIn != null)
            {
                await StopActivatedPlugInAsync(activatedPlugIn);
            }
        }

        private async Task StopActivatedPlugInAsync(ActivatedPlugIn activatedPlugIn)
        {
            throw new NotImplementedException();
        }
    }
}