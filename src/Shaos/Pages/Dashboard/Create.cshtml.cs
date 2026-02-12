using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Pages.Dashboard
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
        ViewData["ParameterId"] = new SelectList(_context.Set<BaseParameter>(), "Id", "Discriminator");
            return Page();
        }

        [BindProperty]
        public DashboardConfiguration DashboardConfiguration { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.DashboardConfigurations.Add(DashboardConfiguration);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
