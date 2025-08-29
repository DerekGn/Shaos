using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shaos.Extensions;
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
            if (string.IsNullOrWhiteSpace(FileName))
            {
                ModelState.AddModelError(string.Empty, "The package file name is empty");
            }
            else
            {
                PackageInformation = _plugInService.ExtractPackageInformation(FileName);
            }
        }

        public IActionResult OnPostCancel(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                PackageInformation = _plugInService.ExtractPackageInformation(fileName.SanitizeFileName());

                _plugInService.DeletePlugInPackage(PackageInformation.PackagePath,
                                                   PackageInformation.ExtractedPath);
            }

            return RedirectToPage("./Index");
        }

        public void OnPostSave()
        {
            //_logger.LogInformation("Save");
        }
    }
}
