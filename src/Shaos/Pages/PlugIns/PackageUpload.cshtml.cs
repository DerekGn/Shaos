/*
* MIT License
*
* Copyright (c) 2025 Derek Goslin https://github.com/DerekGn
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

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

                _logger.LogWarning(ex, "An unhandled exception occurred");
            }

            return Page();
        }
    }
}