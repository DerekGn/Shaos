using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shaos.Services.SystemInformation;

namespace Shaos.Pages.System.Information
{
    public class IndexModel : PageModel
    {
        private readonly ISystemService _systemService;

        public IndexModel(ISystemService systemService)
        {
            _systemService = systemService;
        }

        [BindProperty]
        public RuntimeInformation OsInformation { get; private set; }

        [BindProperty]
        public ProcessInformation ProcessInformation { get; private set; }

        [BindProperty]
        public SystemEnvironment SystemEnvironment { get; private set; }

        public void OnGet()
        {
            SystemEnvironment = _systemService.GetEnvironment();

            ProcessInformation = _systemService.GetProcessInformation();

            OsInformation = _systemService.GetOsInformation();
        }
    }
}