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
using Shaos.Repository.Exceptions;
using Shaos.Repository.Models;

namespace Shaos.Pages.PlugIns
{
    public class CreateModel : PageModel
    {
        private readonly IShaosRepository _repository;

        public CreateModel(IShaosRepository repository)
        {
            ArgumentNullException.ThrowIfNull(repository);

            _repository = repository;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public PlugIn PlugIn { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            ModelState.Remove($"{nameof(PlugIn)}.{nameof(PlugIn.CreatedDate)}");
            ModelState.Remove($"{nameof(PlugIn)}.{nameof(PlugIn.UpdatedDate)}");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _repository.CreatePlugInAsync(PlugIn,
                                                    cancellationToken);

                return RedirectToPage("./Index");
            }
            catch (ShaosNameExistsException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return Page();
            }
        }
    }
}
