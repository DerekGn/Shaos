using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shaos.Extensions;

namespace Shaos.Pages.Parameters
{
    public class HistoryModel : PageModel
    {
        public HistoryModel()
        {
            var offsetUtcNow = DateTimeOffset
                .UtcNow
                .Truncate(TimeSpan.FromMinutes(1));

            StartDateTime = offsetUtcNow.Subtract(TimeSpan.FromHours(24));
            EndDateTime = offsetUtcNow;
        }

        [BindProperty]
        public HistoryData Data { get; set; }

        [BindProperty]
        public DateTimeOffset EndDateTime { get; set; }

        [BindProperty]
        public DateTimeOffset StartDateTime { get; set; }

        public void OnGet()
        {
        }

        public async Task OnPostApplyAsync(CancellationToken cancellationToken)
        {
        }
    }
}