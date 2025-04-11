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
using Microsoft.EntityFrameworkCore;
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Sdk;
using Shaos.Services;
using Shaos.Services.Validation;

namespace Shaos.Pages.PlugIns
{
    public class PackageModel : PageModel
    {
        private readonly ICodeFileValidationService _codeFileValidationService;
        private readonly ShaosDbContext _context;
        private readonly IPlugInService _plugInService;

        public PackageModel(
            ShaosDbContext context,
            IPlugInService plugInService,
            ICodeFileValidationService codeFileValidationService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _plugInService = plugInService ?? throw new ArgumentNullException(nameof(plugInService));
            _codeFileValidationService = codeFileValidationService ?? throw new ArgumentNullException(nameof(codeFileValidationService));
        }

        [BindProperty]
        public Package? Package { get; set; } = default!;

        [BindProperty]
        public IFormFile PackageFile { get; set; } = default!;

        [BindProperty]
        public PlugIn? PlugIn { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plugin = await _context.PlugIns.FirstOrDefaultAsync(m => m.Id == id);
            if (plugin == null)
            {
                return NotFound();
            }
            PlugIn = plugin;
            Package = plugin.Package;

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string? validation = ValidatePackageFile(PackageFile);

            if (validation != null)
            {
                ModelState.AddModelError(string.Empty, validation);

                PlugIn = await _context!.PlugIns!.FirstOrDefaultAsync(m => m.Id == PlugIn!.Id, cancellationToken);
                return Page();
            }

            string? packageValidation = await ValidatePackageAsync(cancellationToken);

            if (packageValidation != null)
            {
                ModelState.AddModelError(string.Empty, packageValidation);

                PlugIn = await _context!.PlugIns!.FirstOrDefaultAsync(m => m.Id == PlugIn!.Id, cancellationToken);
                return Page();
            }

            return RedirectToPage("./Index");
        }

        private string? ValidatePackageFile(IFormFile formFile)
        {
            var validationResult = _codeFileValidationService.ValidateFile(formFile);
            string? validation = null;

            switch (validationResult)
            {
                case FileValidationResult.Success:
                    break;

                case FileValidationResult.FileNameEmpty:
                    validation = "File name is empty";
                    break;

                case FileValidationResult.InvalidContentType:
                    validation = $"File: [{formFile.FileName}] invalid content type";
                    break;

                case FileValidationResult.InvalidFileName:
                    validation = $"File: [{formFile.Name}] has invalid length";
                    break;

                case FileValidationResult.InvalidFileLength:
                    validation = $"File: [{formFile.Name}] has invalid type";
                    break;

                default:
                    break;
            }

            return validation;
        }

        private async Task<string?> ValidatePackageAsync(CancellationToken cancellationToken = default)
        {
            var packageUploadResult = await _plugInService
               .UploadPlugInPackageAsync(PlugIn!.Id, PackageFile.FileName, PackageFile.OpenReadStream(), cancellationToken);

            string? result = null;

            switch (packageUploadResult)
            {
                case UploadPackageResult.Success:
                    break;

                case UploadPackageResult.NoValidPlugInFile:
                    result = $"No valid assembly file found [{PackageFile.FileName}]";
                    break;

                case UploadPackageResult.PlugInRunning:
                    result = $"The PlugIn [{PlugIn.Name}] currently has running instances";
                    break;

                case UploadPackageResult.NoValidPlugInType:
                    result = $"No valid [{nameof(IPlugIn)}] implementation found in package file [{PackageFile.FileName}]";
                    break;

                default:
                    break;
            }

            return result;
        }
    }
}