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
using Shaos.Services.Extensions;
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

        /// <inheritdoc/>
        public IEnumerable<string> ExtractPackage(string sourcePackage, string targetFolder)
        {
            sourcePackage.ThrowIfNullOrEmpty(nameof(sourcePackage));
            targetFolder.ThrowIfNullOrEmpty(nameof(targetFolder));

            var targetPath = Path.Combine(_options.Value.PackagesPath, targetFolder);

            ZipFile.ExtractToDirectory(sourcePackage, targetPath, true);

            return Directory.EnumerateFiles(targetPath);
        }

        /// <inheritdoc/>
        public bool PackageExists(string fileName)
        {
            fileName.ThrowIfNullOrEmpty(nameof(fileName));

            return File.Exists(Path.Combine(_options.Value.PackagesPath, fileName));
        }

        ///// <inheritdoc/>
        //public void DeletePlugInPackageFolder(int plugInId)
        //{
        //    var packageFolder = Path.Combine(_options.Value.PlugInArchivesPath, plugInId.ToString());

        //    if (!Directory.Exists(packageFolder))
        //    {
        //        _logger.LogDebug("Deleting folder [{Folder}]", packageFolder);
        //        Directory.Delete(packageFolder, true);
        //    }
        //    else
        //    {
        //        _logger.LogDebug("Folder [{Folder}] Does Not Exist", packageFolder);
        //    }
        //}

        ///// <inheritdoc/>
        //public Stream? GetNuGetPackageStream(int id, string fileName)
        //{
        //    Stream? stream = null;

        //    var nugetFilePath = GetPlugInNuGetPackagePath(id, fileName);

        //    if (Path.Exists(nugetFilePath))
        //    {
        //        _logger.LogDebug("Opening File Stream: [{File}]", nugetFilePath);
        //        stream = File.Open(nugetFilePath, FileMode.Open, FileAccess.Read);
        //    }
        //    else
        //    {
        //        _logger.LogWarning("File not Found [{File}]", nugetFilePath);
        //    }

        //    return stream;
        //}

        ///// <inheritdoc/>
        //public string GetPlugInNuGetPackagePath(int id, string fileName)
        //{
        //    var packageFolder = Path.Combine(_options.Value.NuGetPackagesPath, id.ToString());

        //    return Path.Combine(packageFolder, fileName);
        //}

        /// <inheritdoc/>
        public async Task<string> WritePlugInPackageFileStreamAsync(
            int plugInId,
            string fileName,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            fileName.ThrowIfNullOrEmpty(nameof(fileName));

            if (!Directory.Exists(_options.Value.PackagesPath))
            {
                _logger.LogDebug("Creating folder [{Folder}]", _options.Value.PackagesPath);

                Directory.CreateDirectory(_options.Value.PackagesPath);
            }

            _logger.LogInformation("Writing File: [{File}] To [{Folder}]", fileName, _options.Value.PackagesPath);

            var packageFilePath = Path.Combine(_options.Value.PackagesPath, fileName);

            using var outputStream = File.Open(packageFilePath, FileMode.OpenOrCreate, FileAccess.Write);

            await stream.CopyToAsync(outputStream, cancellationToken);

            return packageFilePath;
        }
    }
}