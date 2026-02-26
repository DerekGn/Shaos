using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shaos.Repository;
using Shaos.Repository.Models;

namespace Shaos.Pages.System.Dashboard
{
    public class DeleteModel : PageModel
    {
        private readonly ShaosDbContext _context;

        public DeleteModel(ShaosDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DashboardParameter DashboardParameter { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dashboardparameter = await _context.DashboardParameters.FirstOrDefaultAsync(m => m.Id == id);

            if (dashboardparameter is not null)
            {
                DashboardParameter = dashboardparameter;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dashboardparameter = await _context.DashboardParameters.FindAsync(id);
            if (dashboardparameter != null)
            {
                DashboardParameter = dashboardparameter;
                _context.DashboardParameters.Remove(DashboardParameter);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
