using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        public string? PlugInFile { get; set; }

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
                                                                                PackageDetails.PlugInFileName);

                PlugInFile = PackageDetails.PlugInFileName;
            }
        }

        public IActionResult OnPostCancel(string? fileName,
                                          string? extractedPath)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                _fileStoreService.DeletePackage(fileName);
            }

            if (!string.IsNullOrWhiteSpace(extractedPath))
            {
                _fileStoreService.DeletePlugInFiles(extractedPath);
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostSaveAsync(string? plugInFile,
                                                         string? PlugInDirectory,
                                                         CancellationToken cancellationToken = default)
        {
            PlugInTypeInformation = _plugInService.GetPlugInTypeInformation(PlugInDirectory!,
                                                                            plugInFile!);

#warning Handle duplicate name
            await _plugInService.CreatePlugInAsync(PlugInDirectory,
                                                   plugInFile,
                                                   cancellationToken);

            return RedirectToPage("./Index");
        }
    }
}