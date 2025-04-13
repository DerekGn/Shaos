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
using Shaos.Repository.Models;
using Shaos.Services.Exceptions;
using Shaos.Services.Repositories;

namespace Shaos.Pages.PlugInInstances
{
    public class CreateModel : PageModel
    {
        private readonly IPlugInRepository _plugInRepository;
        private readonly IPlugInInstanceRepository _plugInInstanceRepository;

        public CreateModel(
            IPlugInRepository plugInRepository,
            IPlugInInstanceRepository plugInInstanceRepository)
        {
            _plugInRepository = plugInRepository ?? throw new ArgumentNullException(nameof(plugInRepository));
            _plugInInstanceRepository = plugInInstanceRepository ?? throw new ArgumentNullException(nameof(plugInInstanceRepository));
        }

        [BindProperty]
        public int Id { get; set; } = default!;

        [BindProperty]
        public PlugInInstance PlugInInstance { get; set; } = default!;

        public IActionResult OnGetAsync(int id)
        {
            Id = id;

            return Page();
        }

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
        {
            ModelState.Remove($"{nameof(PlugInInstance)}.{nameof(PlugInInstance.CreatedDate)}");
            ModelState.Remove($"{nameof(PlugInInstance)}.{nameof(PlugInInstance.UpdatedDate)}");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var plugIn = await _plugInRepository.GetByIdAsync(Id, false, cancellationToken: cancellationToken);

                await _plugInInstanceRepository.CreateAsync(plugIn!, PlugInInstance, cancellationToken);
            }
            catch (PlugInInstanceNameExistsException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return Page();
            }

            return RedirectToPage("./Index", new { id = Id.ToString() });
        }
    }
}