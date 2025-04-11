using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Services.Store;

namespace Shaos.Pages.PlugInInstances
{
    public class EditModel : PageModel
    {
        private readonly IStore _store;

        public EditModel(IStore store)
        {
            _store = store;
        }

        [BindProperty]
        public PlugInInstance PlugInInstance { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plugininstance = await _store.GetPlugInInstanceByIdAsync(id, cancellationToken);
            if (plugininstance == null)
            {
                return NotFound();
            }
            PlugInInstance = plugininstance;
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


            return RedirectToPage("./Index");
        }
    }
}
