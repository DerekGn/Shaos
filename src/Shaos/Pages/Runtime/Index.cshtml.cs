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
using Shaos.Paging;
using Shaos.Services.Runtime;

namespace Shaos.Pages.Runtime
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IRuntimeService _runtimeService;

        public IndexModel(
            IConfiguration configuration,
            IRuntimeService runtimeService)
        {
            _runtimeService = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [BindProperty]
        public PaginatedList<Instance> Instances { get; set; } = default!;

        //[BindProperty]
        //public IList<PlugInInstance> PlugInInstance { get; set; } = default!;

        public async Task OnGetAsync(
            int? pageIndex)
        {
            Instances = PaginatedList<Instance>.Create(
                _runtimeService.GetInstances().AsQueryable(),
                pageIndex ?? 1,
                _configuration.GetValue("PageSize", 5));
        }
    }
}
