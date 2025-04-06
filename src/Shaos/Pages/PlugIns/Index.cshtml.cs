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

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shaos.Paging;
using Shaos.Repository;
using Shaos.Repository.Models;

namespace Shaos.Pages.PlugIns
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ShaosDbContext _context;

        public IndexModel(
            ShaosDbContext context,
            IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string CurrentFilter { get; set; } = string.Empty;

        public string CurrentSort { get; set; } = string.Empty;

        public string IdSort { get; set; } = string.Empty;

        public string NameSort { get; set; } = string.Empty;

        public PaginatedList<PlugIn> PlugIns { get; set; } = default!;

        public async Task OnGetAsync(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageIndex)
        {
            CurrentSort = sortOrder;
            NameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            IdSort = sortOrder == nameof(PlugIn.Id) ? "id_desc" : nameof(PlugIn.Id);

            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;

            IQueryable<PlugIn> queryablePlugIns = from p in _context.PlugIns
                                                  select p;

            var pageSize = _configuration.GetValue("PageSize", 5);

            if (!String.IsNullOrEmpty(searchString))
            {
                queryablePlugIns = queryablePlugIns.Where(_ => _.Name!.ToLower().Contains(searchString.ToLower()));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    queryablePlugIns = queryablePlugIns.OrderByDescending(_ => _.Name);
                    break;
                case nameof(PlugIn.Name):
                    queryablePlugIns = queryablePlugIns.OrderBy(_ => _.Name);
                    break;
                case "ar_desc":
                    queryablePlugIns = queryablePlugIns.OrderByDescending(_ => _.Id);
                    break;
                case nameof(PlugIn.Id):
                    queryablePlugIns = queryablePlugIns.OrderBy(_ => _.Id);
                    break;
                default:
                    break;
            }

            PlugIns = await PaginatedList<PlugIn>
                .CreateAsync(
                    queryablePlugIns
                        .AsNoTracking()
                        .Include(_ => _.Package)
                        .Include(_ => _.Instances), pageIndex ?? 1, pageSize);
        }
    }
}