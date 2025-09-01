using Microsoft.AspNetCore.Mvc;
using Shaos.Repository.Exceptions;
using Shaos.Services;
using Shaos.Services.IO;

namespace Shaos.Pages.PlugIns
{
    public class CreatePackageInformationModel : BasePackageInformationModel
    {
        public CreatePackageInformationModel(IPlugInService plugInService,
                                             IFileStoreService fileStoreService) : base(plugInService, fileStoreService)
        {
        }

        public async Task<IActionResult> OnPostSaveAsync(string packageFileName,
                                                         string plugInDirectory,
                                                         string plugInAssemblyFile,
                                                         CancellationToken cancellationToken = default)
        {
            try
            {
                await PlugInService.CreatePlugInAsync(packageFileName,
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