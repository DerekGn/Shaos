using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shaos.Repository;
using Shaos.Repository.Models;

namespace Shaos.Pages.PlugIns
{
    public class CreateModel : PageModel
    {
        private readonly Shaos.Repository.ShaosDbContext _context;

        public CreateModel(Shaos.Repository.ShaosDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public PlugIn PlugIn { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.PlugIns.Add(PlugIn);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
