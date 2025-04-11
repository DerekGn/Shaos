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
    public class DetailsModel : PageModel
    {
        private readonly Shaos.Repository.ShaosDbContext _context;

        public DetailsModel(Shaos.Repository.ShaosDbContext context)
        {
            _context = context;
        }

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
    }
}
