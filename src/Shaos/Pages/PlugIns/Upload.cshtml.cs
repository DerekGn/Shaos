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
using Shaos.Sdk;
using Shaos.Services;
using Shaos.Services.Exceptions;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Validation;
using Shaos.Services.Validation;

namespace Shaos.Pages.PlugIns
{
    public class UploadModel : PageModel
    {
        private readonly ILogger<UploadModel> _logger;
        private readonly IPlugInService _plugInService;
        private readonly IZipFileValidationService _zipFileValidationService;

        public UploadModel(ILogger<UploadModel> logger,
                           IPlugInService plugInService,
                           IZipFileValidationService zipFileValidationService)
        {
            _logger = logger;
            _plugInService = plugInService;
            _zipFileValidationService = zipFileValidationService;
        }

        [BindProperty]
        public IFormFile PackageFile { get; set; } = default!;

        [BindProperty]
        public PlugInInformation? PlugInInformation { get; set; } = default!;

        [BindProperty]
        public bool SaveEnable { get; set; } = default!;

        public async Task<IActionResult> OnPostUpload(CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                _zipFileValidationService.ValidateFile(PackageFile);

                PlugInInformation = await _plugInService.ExtractPlugInInformationAsync(PackageFile.FileName,
                                                                                       PackageFile.OpenReadStream(),
                                                                                       cancellationToken);

                SaveEnable = true;
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
            catch (NoValidPlugInAssemblyFoundException)
            {
                ModelState.AddModelError(string.Empty, $"No valid assembly file found [{PackageFile.FileName}]");
            }
            catch (PlugInTypeNotFoundException)
            {
                ModelState.AddModelError(string.Empty, $"No valid [{nameof(IPlugIn)}] implementation found in package file [{PackageFile.FileName}]");
            }
            catch (PlugInTypesFoundException)
            {
                ModelState.AddModelError(string.Empty, $"Multiple [{nameof(IPlugIn)}] implementations found in package file [{PackageFile.FileName}]");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Exception occurred check the logs");

                _logger.LogWarning(ex, "A exception occurred");
            }

            return Page();
        }

        public IActionResult OnPostCancel()
        {
            return RedirectToPage("./Index");
        }

        public void OnPostSave()
        {
            _logger.LogInformation("Save");
        }
    }
}