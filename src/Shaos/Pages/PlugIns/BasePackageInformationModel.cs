using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shaos.Services;
using Shaos.Services.IO;
using Shaos.Services.Runtime.Validation;

namespace Shaos.Pages.PlugIns
{
    public abstract class BasePackageInformationModel : PageModel
    {
        protected readonly IFileStoreService FileStoreService;
        protected readonly IPlugInService PlugInService;

        protected BasePackageInformationModel(IPlugInService plugInService,
                                             IFileStoreService fileStoreService)
        {
            PlugInService = plugInService;
            FileStoreService = fileStoreService;
        }

        [BindProperty]
        public PackageDetails? PackageDetails { get; set; }

        [BindProperty]
        public PlugInTypeInformation? PlugInTypeInformation { get; set; }

        public void OnGet(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                ModelState.AddModelError(string.Empty, "The package file name is empty");
            }
            else
            {
                PackageDetails = PlugInService.ExtractPackage(fileName);

                PlugInTypeInformation = PlugInService.GetPlugInTypeInformation(PackageDetails.PlugInDirectory,
                                                                               PackageDetails.PlugInAssemblyFileName);
            }
        }

        public IActionResult OnPostCancel(string packageFileName,
                                          string plugInDirectory)
        {
            if (!string.IsNullOrWhiteSpace(packageFileName))
            {
                FileStoreService.DeletePackage(packageFileName);
            }

            if (!string.IsNullOrWhiteSpace(plugInDirectory))
            {
                FileStoreService.DeletePlugDirectory(plugInDirectory);
            }

            return RedirectToPage("./Index");
        }
    }
}
