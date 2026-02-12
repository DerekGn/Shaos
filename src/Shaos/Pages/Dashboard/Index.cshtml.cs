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
    public class IndexModel : PageModel
    {
        private readonly Shaos.Repository.ShaosDbContext _context;

        public IndexModel(Shaos.Repository.ShaosDbContext context)
        {
            _context = context;
        }

        public IList<DashboardConfiguration> DashboardConfiguration { get;set; } = default!;

        public async Task OnGetAsync()
        {
            DashboardConfiguration = await _context.DashboardConfigurations
                .Include(d => d.Parameter).ToListAsync();
        }
    }
}
