using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Shaos.Pages.Parameters
{
    public class HistoryModel : PageModel
    {
        public HistoryModel()
        {
            StartDateTime = DateTime.UtcNow.Subtract(TimeSpan.FromHours(24));
            EndDateTime = DateTime.UtcNow;
        }

        [BindProperty]
        public DateTime EndDateTime { get; set; }

        [BindProperty]
        public DateTime StartDateTime { get; set; }

        public void OnGet()
        {
        }

        public async Task OnPostApplyAsync(CancellationToken cancellationToken)
        {
        }
    }
}