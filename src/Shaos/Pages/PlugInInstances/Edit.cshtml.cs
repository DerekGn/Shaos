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

namespace Shaos.Pages.PlugInInstances
{
    public class EditModel : PageModel
    {
        private readonly Shaos.Repository.ShaosDbContext _context;

        public EditModel(Shaos.Repository.ShaosDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PlugInInstance PlugInInstance { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plugininstance =  await _context.PlugInInstances.FirstOrDefaultAsync(m => m.Id == id);
            if (plugininstance == null)
            {
                return NotFound();
            }
            PlugInInstance = plugininstance;
           ViewData["PlugInId"] = new SelectList(_context.PlugIns, "Id", "Description");
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

            _context.Attach(PlugInInstance).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlugInInstanceExists(PlugInInstance.Id))
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

        private bool PlugInInstanceExists(int id)
        {
            return _context.PlugInInstances.Any(e => e.Id == id);
        }
    }
}
