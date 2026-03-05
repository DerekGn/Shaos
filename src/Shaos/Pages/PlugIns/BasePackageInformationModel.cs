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
using Shaos.Services;
using Shaos.Services.IO;
using Shaos.Services.Runtime.Validation;

namespace Shaos.Pages.PlugIns
{
    public abstract class BasePackageInformationModel : PageModel
    {
        protected readonly IFileStoreService FileStoreService;
        protected readonly IPlugInService PlugInService;

        protected BasePackageInformationModel(IFileStoreService fileStoreService,
                                              IPlugInService plugInService)
        {
            PlugInService = plugInService;
            FileStoreService = fileStoreService;
        }

        [BindProperty]
        public PackageDetails? PackageDetails { get; set; }

        [BindProperty]
        public PlugInTypeInformation? PlugInTypeInformation { get; set; }

        public void OnGet(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                ModelState.AddModelError(string.Empty, "The package file name is empty");
            }
            else
            {
                PackageDetails = PlugInService.ExtractPackage(fileName);

                PlugInTypeInformation = PlugInService.GetPlugInTypeInformation(PackageDetails.PlugInDirectory,
                                                                               PackageDetails.PlugInAssemblyFileName);
            }
        }

        public IActionResult OnPostCancel(string packageFileName,
                                          string plugInDirectory)
        {
            if (!string.IsNullOrWhiteSpace(packageFileName))
            {
                FileStoreService.DeletePackage(packageFileName);
            }

            if (!string.IsNullOrWhiteSpace(plugInDirectory))
            {
                FileStoreService.DeletePlugDirectory(plugInDirectory);
            }

            return RedirectToPage("./Index");
        }
    }
}