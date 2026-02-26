using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shaos.Repository;
using Shaos.Repository.Models;

namespace Shaos.Pages.System.Dashboard
{
    public class CreateModel : PageModel
    {
        private readonly ShaosDbContext _context;

        public CreateModel(ShaosDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public DashboardParameter DashboardParameter { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.DashboardParameters.Add(DashboardParameter);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
