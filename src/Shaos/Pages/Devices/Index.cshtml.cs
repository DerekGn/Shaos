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
using Shaos.Paging;
using Shaos.Repository;
using Shaos.Repository.Models.Devices;
using System.Linq.Expressions;

namespace Shaos.Pages.Devices
{
    public class IndexModel : PaginatedModel<Device>
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository _repository;

        public IndexModel(IConfiguration configuration,
                          IRepository repository)
        {
            _configuration = configuration;
            _repository = repository;
        }

        [BindProperty]
        public int Id { get; set; } = default!;

        public async Task OnGetAsync(int id,
                                     string sortOrder,
                                     string currentFilter,
                                     string searchString,
                                     int? pageIndex,
                                     CancellationToken cancellationToken = default)
        {
            Id = id;

            CurrentSort = sortOrder;
            NameSort = string.IsNullOrEmpty(sortOrder) ? NameDescending : "";
            IdSort = sortOrder == nameof(Device.Id) ? IdentifierDescending : nameof(Device.Id);

            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;

            Expression<Func<Device, bool>>? filter = null;
            Func<IQueryable<Device>, IOrderedQueryable<Device>>? orderBy = null;

            if (!string.IsNullOrEmpty(searchString))
            {
                filter = _ => _.Id == id && _.Name!.Contains(searchString, StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                filter = _ => _.Id == id;
            }

            switch (sortOrder)
            {
                case NameDescending:
                    orderBy = _ => _.OrderByDescending(_ => _.Name);
                    break;

                case nameof(Device.Name):
                    orderBy = _ => _.OrderBy(_ => _.Name);
                    break;

                case IdentifierDescending:
                    orderBy = _ => _.OrderByDescending(_ => _.Id);
                    break;

                case nameof(Device.Id):
                    orderBy = _ => _.OrderBy(_ => _.Id);
                    break;

                default:
                    break;
            }

            var queryable = _repository.GetQueryable(filter,
                                                     orderBy);

            List = await PaginatedList<Device>.CreateAsync(queryable,
                                                           pageIndex ?? 1,
                                                           _configuration.GetValue("PageSize", 5),
                                                           cancellationToken);
        }
    }
}
