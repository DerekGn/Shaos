using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shaos.Repository;
using Shaos.Repository.Models;

namespace Shaos.Pages.PlugInInstances
{
    public class DeleteModel : PageModel
    {
        private readonly Shaos.Repository.ShaosDbContext _context;

        public DeleteModel(Shaos.Repository.ShaosDbContext context)
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

            var plugininstance = await _context.PlugInInstances.FirstOrDefaultAsync(m => m.Id == id);

            if (plugininstance == null)
            {
                return NotFound();
            }
            else
            {
                PlugInInstance = plugininstance;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plugininstance = await _context.PlugInInstances.FindAsync(id);
            if (plugininstance != null)
            {
                PlugInInstance = plugininstance;
                _context.PlugInInstances.Remove(PlugInInstance);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
