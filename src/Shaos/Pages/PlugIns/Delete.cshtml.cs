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
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Services;
using Shaos.Services.Exceptions;

namespace Shaos.Pages.PlugIns
{
    public class DeleteModel : PageModel
    {
        private readonly IPlugInService _plugInService;
        private readonly IPlugInRepository _repository;

        public DeleteModel(
            IPlugInRepository repository,
            IPlugInService plugInService)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(plugInService);

            _repository = repository;
            _plugInService = plugInService;
        }

        [BindProperty]
        public PlugIn PlugIn { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id,
                                                    CancellationToken cancellationToken)
        {
            var plugin = await _repository.GetByIdAsync<PlugIn>(id,
                                                                cancellationToken: cancellationToken);

            if (plugin == null)
            {
                ModelState.AddModelError("NotFound", $"PlugIn: [{id}] was not found");
            }
            else
            {
                PlugIn = plugin;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id,
                                                     CancellationToken cancellationToken)
        {
            try
            {
                await _plugInService.DeletePlugInAsync(id,
                                                       cancellationToken);

                return RedirectToPage("./Index");
            }
            catch (PlugInInstanceRunningException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return Page();
            }
        }
    }
}