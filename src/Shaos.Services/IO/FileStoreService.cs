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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shaos.Services.Options;

namespace Shaos.Services.IO
{
    public class FileStoreService : IFileStoreService
    {
        private readonly ILogger<FileStoreService> _logger;
        private readonly IOptions<FileStoreOptions> _options;

        public FileStoreService(
            ILogger<FileStoreService> logger,
            IOptions<FileStoreOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public void DeletePlugInPackageFolder(int plugInId)
        {
            var packageFolder = Path.Combine(_options.Value.NuGetPackagesPath, plugInId.ToString());

            if (!Directory.Exists(packageFolder))
            {
                _logger.LogDebug("Deleting folder [{Folder}]", packageFolder);
                Directory.Delete(packageFolder, true);
            }
            else
            {
                _logger.LogDebug("Folder [{Folder}] Does Not Exist", packageFolder);
            }
        }

        /// <inheritdoc/>
        public async Task<string?> WritePlugInNuGetPackageFileStreamAsync(
            int plugInId,
            string fileName,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            var packageFolder = Path.Combine(_options.Value.NuGetPackagesPath, plugInId.ToString());

            if (!Directory.Exists(packageFolder))
            {
                _logger.LogDebug("Creating folder [{Folder}]", packageFolder);

                Directory.CreateDirectory(packageFolder);
            }

            _logger.LogInformation("Writing File: [{File}] To [{Folder}]", packageFolder, packageFolder);

            var nugetFilePath = Path.Combine(packageFolder, fileName);

            using var outputStream = File.Open(nugetFilePath, FileMode.OpenOrCreate, FileAccess.Write);

            await stream.CopyToAsync(outputStream, cancellationToken);

            return nugetFilePath;
        }
    }
}