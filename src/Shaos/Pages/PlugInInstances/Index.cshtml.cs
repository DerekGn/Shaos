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
using Microsoft.EntityFrameworkCore;
using Shaos.Paging;
using Shaos.Repository;
using Shaos.Repository.Models;

namespace Shaos.Pages.PlugInInstances
{
    public class IndexModel : PaginatedModel<PlugInInstance>
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

        [BindProperty]
        public int Id { get;set; } = default!;

        public async Task OnGetAsync(
            int id,
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageIndex,
            CancellationToken cancellationToken = default)
        {
            Id = id;

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

            IQueryable<PlugInInstance> queryable = from p in _context.PlugInInstances
                                                   select p;

            var pageSize = _configuration.GetValue("PageSize", 5);

            if (!String.IsNullOrEmpty(searchString))
            {
                queryable = queryable.Where(_ => _.Name!.ToLower().Contains(searchString.ToLower()));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    queryable = queryable.OrderByDescending(_ => _.Name);
                    break;
                case nameof(PlugIn.Name):
                    queryable = queryable.OrderBy(_ => _.Name);
                    break;
                case "ar_desc":
                    queryable = queryable.OrderByDescending(_ => _.Id);
                    break;
                case nameof(PlugIn.Id):
                    queryable = queryable.OrderBy(_ => _.Id);
                    break;
                default:
                    break;
            }

            List = await PaginatedList<PlugInInstance>
                .CreateAsync(queryable.AsNoTracking(), pageIndex ?? 1, pageSize, cancellationToken);
        }
    }
}
