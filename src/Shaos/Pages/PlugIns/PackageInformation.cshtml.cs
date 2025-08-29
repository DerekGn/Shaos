using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shaos.Services;

namespace Shaos.Pages.PlugIns
{
    public class PackageInformationModel : PageModel
    {
        private readonly IPlugInService _plugInService;

        public PackageInformationModel(IPlugInService plugInService)
        {
            _plugInService = plugInService;
        }

        [TempData]
        public string? FileName { get; set; }

        [BindProperty]
        public PackageInformation? PackageInformation { get; set; }

        public void OnGet()
        {
            if(String.IsNullOrWhiteSpace(FileName))
            {
                ModelState.AddModelError(string.Empty, "The package file name is empty");
            }
            else
            {
                PackageInformation = _plugInService.ExtractPackageInformation(FileName);
            }
        }
    }
}
