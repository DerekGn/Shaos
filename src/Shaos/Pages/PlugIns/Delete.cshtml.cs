using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shaos.Repository;
using Shaos.Repository.Models;

namespace Shaos.Pages.PlugIns
{
    public class DeleteModel : PageModel
    {
        private readonly Shaos.Repository.ShaosDbContext _context;

        public DeleteModel(Shaos.Repository.ShaosDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PlugIn PlugIn { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plugin = await _context.PlugIns.FirstOrDefaultAsync(m => m.Id == id);

            if (plugin == null)
            {
                return NotFound();
            }
            else
            {
                PlugIn = plugin;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plugin = await _context.PlugIns.FindAsync(id);
            if (plugin != null)
            {
                PlugIn = plugin;
                _context.PlugIns.Remove(PlugIn);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
