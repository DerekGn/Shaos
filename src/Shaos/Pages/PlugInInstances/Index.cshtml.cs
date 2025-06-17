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
using Shaos.Repository.Models;
using System.Linq.Expressions;

namespace Shaos.Pages.PlugInInstances
{
    public class IndexModel : PaginatedModel<PlugInInstance>
    {
        private readonly IConfiguration _configuration;
        private readonly IShaosRepository _repository;

        public IndexModel(IConfiguration configuration,
                          IShaosRepository repository)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(repository);
            
            _configuration = configuration;
            _repository = repository;
        }

        [BindProperty]
        public int Id { get;set; } = default!;

        public async Task OnGetAsync(int id,
                                     string sortOrder,
                                     string currentFilter,
                                     string searchString,
                                     int? pageIndex,
                                     CancellationToken cancellationToken = default)
        {
            Id = id;

            CurrentSort = sortOrder;
            NameSort = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
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

            Expression<Func<PlugInInstance, bool>>? filter = null;
            Func<IQueryable<PlugInInstance>, IOrderedQueryable<PlugInInstance>>? orderBy = null;

            if (!string.IsNullOrEmpty(searchString))
            {
                filter = _ => _.PlugInId == id && _.Name!.Contains(searchString, StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                filter = _ => _.PlugInId == id;
            }

                switch (sortOrder)
                {
                    case "name_desc":
                        orderBy = _ => _.OrderByDescending(_ => _.Name);
                        break;

                    case nameof(PlugInInstance.Name):
                        orderBy = _ => _.OrderBy(_ => _.Name);
                        break;

                    case "id_desc":
                        orderBy = _ => _.OrderByDescending(_ => _.Id);
                        break;

                    case nameof(PlugInInstance.Id):
                        orderBy = _ => _.OrderBy(_ => _.Id);
                        break;

                    default:
                        break;
                }

            var queryable = _repository.GetQueryable(
                filter,
                orderBy);

            List = await PaginatedList<PlugInInstance>.CreateAsync(queryable,
                                                                   pageIndex ?? 1,
                                                                   _configuration.GetValue("PageSize", 5),
                                                                   cancellationToken);
        }
    }
}
