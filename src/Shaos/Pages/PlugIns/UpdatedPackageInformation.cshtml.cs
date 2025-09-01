using Microsoft.AspNetCore.Mvc;
using Shaos.Services;
using Shaos.Services.IO;

namespace Shaos.Pages.PlugIns
{
    public class UpdatedPackageInformationModel : BasePackageInformationModel
    {
        public UpdatedPackageInformationModel(IPlugInService plugInService,
                                             IFileStoreService fileStoreService) : base(plugInService, fileStoreService)
        {
        }

        [FromRoute]
        public int Id { get; set; }

        public async Task<IActionResult> OnPostSaveAsync(string packageFileName,
                                                         string plugInDirectory,
                                                         string plugInAssemblyFile,
                                                         CancellationToken cancellationToken = default)
        {
            await PlugInService.UpdatePlugInPackageAsync(Id,
                                                         packageFileName,
                                                         plugInDirectory,
                                                         plugInAssemblyFile,
                                                         cancellationToken);

            return RedirectToPage("./Index");
        }
    }
}
