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
using System.IO.Compression;

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

        public void DeletePackage(int id, string fileName)
        {
            var filePath = Path.Combine(Path.Combine(_options.Value.PackagesPath, id.ToString()), fileName);

            if(File.Exists(filePath))
            {
                _logger.LogInformation("Deleting file [{Path}]", filePath);

                File.Delete(filePath);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> ExtractPackage(string sourcePackage, string targetFolder)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(sourcePackage);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(targetFolder);

            var sourcePath = Path.Combine(_options.Value.PackagesPath, sourcePackage);
            var targetPath = Path.Combine(_options.Value.BinariesPath, targetFolder);

            ZipFile.ExtractToDirectory(sourcePath, targetPath, true);

            return Directory.EnumerateFiles(targetPath);
        }

        public string GetAssemblyPath(int id)
        {
            return Path.Combine(_options.Value.BinariesPath, id.ToString());
        }

        /// <inheritdoc/>
        public bool PackageExists(string fileName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fileName);

            return File.Exists(Path.Combine(_options.Value.PackagesPath, fileName));
        }

        /// <inheritdoc/>
        public async Task<string> WritePackageFileStreamAsync(
            int id,
            string packageFileName,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(packageFileName);

            if (!Directory.Exists(_options.Value.PackagesPath))
            {
                _logger.LogDebug("Creating folder [{Folder}]", _options.Value.PackagesPath);

                Directory.CreateDirectory(_options.Value.PackagesPath);
            }

            _logger.LogInformation("Writing Package File: [{File}] To [{Folder}]", packageFileName, _options.Value.PackagesPath);

            var packageFilePath = Path.Combine(_options.Value.PackagesPath, packageFileName);

            using var outputStream = File.Open(packageFilePath, FileMode.OpenOrCreate, FileAccess.Write);

            await stream.CopyToAsync(outputStream, cancellationToken);

            return packageFilePath;
        }
    }
}