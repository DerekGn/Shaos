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
using Shaos.Extensions;
using Shaos.Pages.System.Dashboard.Model;
using Shaos.Repository;
using Shaos.Repository.Exceptions;
using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Pages.System.Dashboard
{
    public class CreateModel : DashboardItemPageModel
    {
        public CreateModel(IShaosRepository repository) : base(repository) { }

        [BindProperty]
        public DashboardItemModel DashboardItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            PopulateParametersDropDownList();

            return Page();
        }

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var parameterId = DashboardItem.Parameter.Id;

            var parameter = await Repository.GetFirstOrDefaultAsync<BaseParameter>(_ => _.Id == parameterId,
                                                                                   withNoTracking: false,
                                                                                   cancellationToken: cancellationToken);

            if (parameter == null)
            {
                ModelState.AddModelError("NotFound", $"Parameter: [{parameterId}] was not found");
            }
            else
            {
                var dashboardItem = DashboardItem.FromModel();
                dashboardItem.Parameter = parameter;
                await Repository.AddAsync(dashboardItem, cancellationToken);

                try
                {
                    await Repository.SaveChangesAsync(cancellationToken);
                }
                catch (DuplicateEntityException)
                {
                    ModelState.AddModelError(string.Empty, $"Dashboard Item already exists. Label: {DashboardItem.Label} Name: {parameter.Name}");

                    return Page();
                }
            }

            return RedirectToPage("./Index");
        }
    }
}