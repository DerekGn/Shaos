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
using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Pages.Dashboard
{
    public class EditModel : PageModel
    {
        private readonly Shaos.Repository.ShaosDbContext _context;

        public EditModel(Shaos.Repository.ShaosDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DashboardConfiguration DashboardConfiguration { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dashboardconfiguration =  await _context.DashboardConfigurations.FirstOrDefaultAsync(m => m.Id == id);
            if (dashboardconfiguration == null)
            {
                return NotFound();
            }
            DashboardConfiguration = dashboardconfiguration;
           ViewData["ParameterId"] = new SelectList(_context.Set<BaseParameter>(), "Id", "Discriminator");
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

            _context.Attach(DashboardConfiguration).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DashboardConfigurationExists(DashboardConfiguration.Id))
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

        private bool DashboardConfigurationExists(int id)
        {
            return _context.DashboardConfigurations.Any(e => e.Id == id);
        }
    }
}
