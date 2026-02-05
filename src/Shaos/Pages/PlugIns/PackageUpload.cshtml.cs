using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shaos.Services.Exceptions;
using Shaos.Services.IO;
using Shaos.Services.Validation;

namespace Shaos.Pages.PlugIns
{
    public partial class PackageUploadModel : PageModel
    {
        private readonly IFileStoreService _fileStoreService;
        private readonly ILogger<PackageUploadModel> _logger;
        private readonly IZipFileValidationService _zipFileValidationService;

        public PackageUploadModel(ILogger<PackageUploadModel> logger,
                                  IFileStoreService fileStoreService,
                                  IZipFileValidationService zipFileValidationService)
        {
            _logger = logger;
            _fileStoreService = fileStoreService;
            _zipFileValidationService = zipFileValidationService;
        }

        [BindProperty]
        public IFormFile PackageFile { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _zipFileValidationService.ValidateFile(PackageFile);

                await _fileStoreService.WritePackageAsync(PackageFile.FileName,
                                                          PackageFile.OpenReadStream(),
                                                          cancellationToken);

                return id == 0 ?
                    RedirectToPage("./CreatePackageInformation", new { PackageFile.FileName }) :
                    RedirectToPage("./UpdatedPackageInformation", new { Id = id, PackageFile.FileName });
            }
            catch (FileContentInvalidException ex)
            {
                ModelState.AddModelError(string.Empty, $"The file [{ex.FileName}] has invalid content type: [{ex.ContentType}]");
            }
            catch (FileLengthInvalidException ex)
            {
                ModelState.AddModelError(string.Empty, $"The file [{ex.FileName}] has invalid file length: [{ex.FileLength}]");
            }
            catch (FileNameEmptyException)
            {
                ModelState.AddModelError(string.Empty, "The file name is empty");
            }
            catch (FileNameInvalidExtensionException ex)
            {
                ModelState.AddModelError(string.Empty, $"The file [{ex.FileName}] has an invalid extension.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Exception occurred check the logs");

                LogWarning(ex);
            }

            return Page();
        }

        [LoggerMessage(Level = LogLevel.Warning, Message = "A exception occurred")]
        private partial void LogWarning(Exception exception);
    }
}