using Microsoft.AspNetCore.Mvc;
using Shaos.Repository.Exceptions;
using Shaos.Services;
using Shaos.Services.Exceptions;
using Shaos.Services.IO;

namespace Shaos.Pages.PlugIns
{
    public class UpdatedPackageInformationModel : BasePackageInformationModel
    {
        public UpdatedPackageInformationModel(IPlugInService plugInService,
                                             IFileStoreService fileStoreService) : base(plugInService, fileStoreService)
        {
        }

        public async Task<IActionResult> OnPostSaveAsync(int id,
                                                         string packageFileName,
                                                         string plugInDirectory,
                                                         string plugInAssemblyFile,
                                                         CancellationToken cancellationToken = default)
        {
            try
            {
                await PlugInService.UpdatePlugInPackageAsync(id,
                                                             packageFileName,
                                                             plugInDirectory,
                                                             plugInAssemblyFile,
                                                             cancellationToken);

                return RedirectToPage("./Index");
            }
            catch (NameExistsException ex)
            {
                ModelState.AddModelError(string.Empty, $"PlugIn Name: [{ex.Name}] already exists");

                return Page();
            }
            catch (PlugInInstancesRunningException ex)
            {
                ModelState.AddModelError(string.Empty, $"There are running PlugIn Instances [{string.Join(",", ex.Ids)}]");
            }

            return Page();
        }
    }
}
