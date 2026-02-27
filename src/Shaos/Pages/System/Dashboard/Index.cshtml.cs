using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shaos.Repository;
using Shaos.Repository.Models;

namespace Shaos.Pages.System.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly ShaosDbContext _context;

        public IndexModel(ShaosDbContext context)
        {
            _context = context;
        }

        public IList<DashboardParameter> DashboardParameter { get;set; } = default!;

        public async Task OnGetAsync()
        {
            DashboardParameter = await _context.DashboardParameters.ToListAsync();
        }
    }
}
