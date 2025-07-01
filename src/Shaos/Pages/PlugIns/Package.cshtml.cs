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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Services;
using Shaos.Services.Exceptions;
using Shaos.Services.Runtime.Validation;
using Shaos.Services.Validation;

namespace Shaos.Pages.PlugIns
{
    public class PackageModel : PageModel
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IPlugInService _plugInService;
        private readonly ILogger<PackageModel> _logger;
        private readonly IShaosRepository _repository;

        public PackageModel(
            IMemoryCache memoryCache,
            ILogger<PackageModel> logger,
            IShaosRepository repository,
            IPlugInService plugInService)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _repository = repository;
            _plugInService = plugInService;
        }

        [BindProperty]
        public PlugInInformation? PlugInInformation { get; set; } = default!;

        public IActionResult OnGet(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _memoryCache.TryGetValue(id, out PlugInInformation? plugInInformation);

            if(plugInInformation == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to fetch PlugInInformation");
                return Page();
            }

            PlugInInformation = plugInInformation;

            _memoryCache.Remove(id);
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
        {
            return Page();
        }
    }
}