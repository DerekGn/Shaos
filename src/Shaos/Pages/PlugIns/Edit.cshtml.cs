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
using Microsoft.EntityFrameworkCore;
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Services.Exceptions;
using Shaos.Services.Store;

namespace Shaos.Pages.PlugIns
{
    public class EditModel : PageModel
    {
        private readonly IStore _store;

        public EditModel(
            IStore store,
            ShaosDbContext context)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        [BindProperty]
        public PlugIn PlugIn { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plugin = await _store.GetPlugInByIdAsync(id.Value);
            if (plugin == null)
            {
                return NotFound();
            }
            PlugIn = plugin;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _store.UpdatePlugInAsync(PlugIn.Id, PlugIn.Name, PlugIn.Description);
            }
            catch (PlugInNameExistsException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return Page();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PlugInExistsAsync(PlugIn.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private async Task<bool> PlugInExistsAsync(int id)
        {
            return await _store.ExistsAsync<PlugIn>(id);
        }
    }
}