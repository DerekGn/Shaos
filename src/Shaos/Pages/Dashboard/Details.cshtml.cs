using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shaos.Repository;
using Shaos.Repository.Models;

namespace Shaos.Pages.Dashboard
{
    public class DetailsModel : PageModel
    {
        private readonly Shaos.Repository.ShaosDbContext _context;

        public DetailsModel(Shaos.Repository.ShaosDbContext context)
        {
            _context = context;
        }

        public DashboardConfiguration DashboardConfiguration { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dashboardconfiguration = await _context.DashboardConfigurations.FirstOrDefaultAsync(m => m.Id == id);

            if (dashboardconfiguration is not null)
            {
                DashboardConfiguration = dashboardconfiguration;

                return Page();
            }

            return NotFound();
        }
    }
}
