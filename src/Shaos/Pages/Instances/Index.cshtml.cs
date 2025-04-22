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
using Shaos.Services.Runtime;
using Shaos.Services.Runtime.Host;

namespace Shaos.Pages.Instances
{
    public class IndexModel : PaginatedModel<Instance>
    {
        private readonly IInstanceHost _instanceHost;
        private readonly IConfiguration _configuration;

        public IndexModel(
            IInstanceHost instanceHost,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(instanceHost);
            ArgumentNullException.ThrowIfNull(configuration);

            _instanceHost = instanceHost;
            _configuration = configuration;
        }

        [BindProperty]
        public string StateSort { get; set; } = string.Empty;

        public void OnGet(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageIndex)
        {
            CurrentSort = sortOrder;
            NameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            IdSort = sortOrder == nameof(Instance.Id) ? "id_desc" : nameof(Instance.Id);
            StateSort = sortOrder == nameof(Instance.State) ? "state_desc" : nameof(Instance.State);

            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;


            var queryable = _instanceHost.Instances.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                queryable = queryable.Where(_ => _.Name.ToLower().Contains(searchString.ToLower()));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    queryable = queryable.OrderByDescending(_ => _.Name);
                    break;

                case nameof(Instance.Name):
                    queryable = queryable.OrderBy(_ => _.Name);
                    break;

                case "id_desc":
                    queryable = queryable.OrderByDescending(_ => _.Id);
                    break;

                case nameof(Instance.Id):
                    queryable = queryable.OrderBy(_ => _.Id);
                    break;

                case "state_desc":
                    queryable = queryable.OrderByDescending(_ => _.State);
                    break;

                case nameof(Instance.State):
                    queryable = queryable.OrderBy(_ => _.State);
                    break;

                default:
                    break;
            }

            List = PaginatedList<Instance>
                .Create(
                    queryable, pageIndex ?? 1,
                    _configuration.GetValue("PageSize", 5));
        }
    }
}
