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
        public PlugInInformation? PlugInInformation { get; set; }

        public void OnGet()
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                ModelState.AddModelError(string.Empty, "The package file name is empty");
            }
            else
            {
                PackageDetails = _plugInService.ExtractPackage(FileName);

                PlugInInformation = _plugInService.GetPackageInformation(PackageDetails.ExtractedPath,
                                                                         PackageDetails.PlugInFile);

                PlugInFile = PackageDetails.PlugInFile;
            }
        }

        public IActionResult OnPostCancel(string? fileName,
                                          string? extractedPath)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                _fileStoreService.DeletePackage(fileName);
            }

            if(!string.IsNullOrWhiteSpace(extractedPath))
            {
                _fileStoreService.DeleteExtractedPackage(extractedPath);
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostSaveAsync(string? plugInFile,
                                                         string? extractedPath,
                                                         CancellationToken cancellationToken = default)
        {
            PlugInInformation = _plugInService.GetPackageInformation(extractedPath!,
                                                                     plugInFile!);

#warning Handle duplicate name
            await _plugInService.CreatePlugInAsync(plugInFile,
                                                   extractedPath,
                                                   cancellationToken);

            return RedirectToPage("./Index");
        }
    }
}