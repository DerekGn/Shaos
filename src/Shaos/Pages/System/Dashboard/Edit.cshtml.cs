using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shaos.Repository;
using Shaos.Repository.Models;

namespace Shaos.Pages.System.Dashboard
{
    public class EditModel : PageModel
    {
        private readonly ShaosDbContext _context;

        public EditModel(ShaosDbContext context)
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

            var dashboardparameter =  await _context.DashboardParameters.FirstOrDefaultAsync(m => m.Id == id);
            if (dashboardparameter == null)
            {
                return NotFound();
            }
            DashboardParameter = dashboardparameter;
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

            _context.Attach(DashboardParameter).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DashboardParameterExists(DashboardParameter.Id))
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

        private bool DashboardParameterExists(int id)
        {
            return _context.DashboardParameters.Any(e => e.Id == id);
        }
    }
}
