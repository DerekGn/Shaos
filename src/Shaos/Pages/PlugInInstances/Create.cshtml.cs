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
using Shaos.Services;
using Shaos.Services.Exceptions;
using Shaos.Services.Repositories;
using System.Threading.Tasks;

namespace Shaos.Pages.PlugInInstances
{
    public class CreateModel : PageModel
    {
        private readonly IPlugInService _plugInService;
        private readonly IPlugInRepository _repository;

        public CreateModel(
            IPlugInService plugInService,
            IPlugInRepository repository)
        {
            ArgumentNullException.ThrowIfNull(plugInService);
            ArgumentNullException.ThrowIfNull(repository);

            _plugInService = plugInService;
            _repository = repository;
        }

        [BindProperty]
        public bool CreateEnabled { get; set; }

        [BindProperty]
        public int Id { get; set; } = default!;

        [BindProperty]
        public PlugInInstance PlugInInstance { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken = default)
        {
            Id = id;

            var plugIn = await _repository.GetByIdAsync(id, true, [nameof(PlugIn.Package)], cancellationToken);

            if (plugIn != null)
            {
                CreateEnabled = !plugIn.Package!.HasConfiguration;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostConfigureAsync(CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _plugInService.CreatePlugInInstanceAsync(Id, PlugInInstance, cancellationToken);
            }
            catch (PlugInInstanceNameExistsException)
            {
                ModelState.AddModelError(string.Empty, $"PlugInInstance Name: [{PlugInInstance.Name}] already exists");

                return Page();
            }

            return RedirectToPage("./Configuration", new { id = PlugInInstance.Id });
        }

        public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _plugInService.CreatePlugInInstanceAsync(Id, PlugInInstance, cancellationToken);
            }
            catch (PlugInInstanceNameExistsException)
            {
                ModelState.AddModelError(string.Empty, $"PlugInInstance Name: [{PlugInInstance.Name}] already exists");

                return Page();
            }

            return RedirectToPage("./Index", new { id = Id });
        }
    }
}