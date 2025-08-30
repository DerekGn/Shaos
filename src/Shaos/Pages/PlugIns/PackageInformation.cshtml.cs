using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shaos.Repository.Exceptions;
using Shaos.Services;
using Shaos.Services.IO;
using Shaos.Services.Runtime.Validation;

namespace Shaos.Pages.PlugIns
{
    public class PackageInformationModel : PageModel
    {
        private readonly IFileStoreService _fileStoreService;
        private readonly IPlugInService _plugInService;

        public PackageInformationModel(IPlugInService plugInService,
                                       IFileStoreService fileStoreService)
        {
            _plugInService = plugInService;
            _fileStoreService = fileStoreService;
        }

        [TempData]
        public string? FileName { get; set; }

        [BindProperty]
        public PackageDetails? PackageDetails { get; set; }

        [TempData]
        public string? PlugInAssemblyFileName { get; set; }

        [BindProperty]
        public PlugInTypeInformation? PlugInTypeInformation { get; set; }

        public void OnGet()
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                ModelState.AddModelError(string.Empty, "The package file name is empty");
            }
            else
            {
                PackageDetails = _plugInService.ExtractPackage(FileName);

                PlugInTypeInformation = _plugInService.GetPlugInTypeInformation(PackageDetails.PlugInDirectory,
                                                                                PackageDetails.PlugInAssemblyFileName);

                PlugInAssemblyFileName = PackageDetails.PlugInAssemblyFileName;
            }
        }

        public IActionResult OnPostCancel(string packageFilename,
                                          string plugInDirectory)
        {
            if (!string.IsNullOrWhiteSpace(packageFilename))
            {
                _fileStoreService.DeletePackage(packageFilename);
            }

            if (!string.IsNullOrWhiteSpace(plugInDirectory))
            {
                _fileStoreService.DeletePlugDirectory(plugInDirectory);
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostSaveAsync(string packageFilename,
                                                         string plugInDirectory,
                                                         string plugInAssemblyFile,
                                                         CancellationToken cancellationToken = default)
        {
            try
            {
                await _plugInService.CreatePlugInAsync(packageFilename,
                                                       plugInDirectory,
                                                       plugInAssemblyFile,
                                                       cancellationToken);
            }
            catch (NameExistsException ex)
            {
                ModelState.AddModelError(string.Empty, $"PlugIn Name: [{ex.Name}] already exists");

                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}