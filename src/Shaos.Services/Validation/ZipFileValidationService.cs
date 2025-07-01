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

using Microsoft.AspNetCore.Http;
using Shaos.Services.Exceptions;

namespace Shaos.Services.Validation
{
    /// <summary>
    /// A zip file validation service
    /// </summary>
    public class ZipFileValidationService : IZipFileValidationService
    {
        internal const string ContentType = "application/x-zip-compressed";
        private readonly string[] permittedExtensions = { ".zip" };

        /// <inheritdoc/>
        public void ValidateFile(IFormFile formFile)
        {
            if (string.IsNullOrWhiteSpace(formFile.ContentType))
            {
                throw new FileContentInvalidException(formFile.FileName, formFile.ContentType);
            }
            else if (!formFile.ContentType.Equals(ContentType, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new FileContentInvalidException(formFile.FileName, formFile.ContentType);
            }
            else if (formFile.Length == 0)
            {
                throw new FileLengthInvalidException(formFile.Length);
            }
            else if (string.IsNullOrEmpty(formFile.FileName))
            {
                throw new FileNameEmptyException();
            }
            else if (!ValidFileName(formFile.FileName))
            {
                throw new FileNameInvalidExtensionException(formFile.FileName);
            }
        }

        private bool ValidFileName(string fileName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fileName);

            var ext = Path
                .GetExtension(fileName)
                .ToLowerInvariant();

            return !string.IsNullOrEmpty(ext) && permittedExtensions.Contains(ext);
        }
    }
}