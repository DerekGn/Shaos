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
    /// <summary>
    /// A file store service that abstracts file store operations
    /// </summary>
    public class FileStoreService : IFileStoreService
    {
        private readonly ILogger<FileStoreService> _logger;
        private readonly IOptions<FileStoreOptions> _options;

        /// <summary>
        /// Create an instance of a <see cref="IFileStoreService"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{FileStoreService}"/> instance</param>
        /// <param name="options">The <see cref="IOptions{FileStoreOptions}"/> instance</param>
        public FileStoreService(ILogger<FileStoreService> logger,
                                IOptions<FileStoreOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        /// <inheritdoc/>
        public void DeletePlugInFiles(string extractedPath)
        {
            var extractedPackagePath = Path.Combine(_options.Value.BinariesPath, extractedPath);

            if (Directory.Exists(extractedPackagePath))
            {
                _logger.LogInformation("Deleting folder [{Path}]", extractedPackagePath);

                Directory.Delete(extractedPackagePath, true);
            }
        }

        /// <inheritdoc/>
        public void DeletePackage(string packageFile)
        {
            var packageFilePath = Path.Combine(_options.Value.PackagesPath, packageFile);

            if (File.Exists(packageFilePath))
            {
                File.Delete(packageFilePath);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> ExtractPackage(int id,
                                                  string packageFileName)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

            var sourcePath = Path.Combine(_options.Value.PackagesPath, id.ToString());
            var targetPath = Path.Combine(_options.Value.BinariesPath, id.ToString());

            _logger.LogInformation("Emptying directory [{TargetPath}]", targetPath);

            targetPath.EmptyDirectory();

            sourcePath = Path.Combine(sourcePath, packageFileName);

            _logger.LogInformation("Extracting package: [{SourcePath}] to [{TargetPath}]",
                sourcePath,
                targetPath);

            ZipFile.ExtractToDirectory(sourcePath, targetPath, true);

            return Directory.EnumerateFiles(targetPath);
        }

        /// <inheritdoc/>
        public void ExtractPackage(string packageFileName,
                                   out string extractedPath,
                                   out IEnumerable<string> files)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(packageFileName);

            extractedPath = Guid.NewGuid().ToString();
            var sourcePath = _options.Value.PackagesPath;
            var targetPath = Path.Combine(_options.Value.BinariesPath, extractedPath);

            sourcePath = Path.Combine(sourcePath, packageFileName);

            _logger.LogInformation("Extracting package: [{SourcePath}] to [{TargetPath}]",
                                   sourcePath,
                                   targetPath);

            ZipFile.ExtractToDirectory(sourcePath, targetPath, true);

            files = Directory.EnumerateFiles(targetPath);
        }

        /// <inheritdoc/>
        public string GetAssemblyPath(string plugInDirectory,
                                      string plugInAssemblyFileName)
        {
            ArgumentNullException.ThrowIfNull(plugInDirectory);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(plugInAssemblyFileName);

            return Path.Combine(Path.Combine(_options.Value.BinariesPath, plugInDirectory), plugInAssemblyFileName);
        }

        /// <inheritdoc/>
        public async Task WritePackageAsync(string packageFileName,
                                            Stream packageFileStream,
                                            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(packageFileStream);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(packageFileName);

            if (_options.Value.PackagesPath.CreateDirectory())
            {
                _logger.LogInformation("Creating packages directory [{Folder}]", _options.Value.PackagesPath);
            }

            var packageFilePath = _options.Value.PackagesPath;

            if (Directory.Exists(packageFilePath))
            {
                _logger.LogInformation("Emptying package directory [{Folder}]", packageFilePath);

                packageFilePath.EmptyDirectory();
            }
            else
            {
                _logger.LogInformation("Creating package directory [{Folder}]", packageFilePath);

                packageFilePath.CreateDirectory();
            }

            packageFilePath = Path.Combine(packageFilePath, packageFileName);

            _logger.LogInformation("Writing Package File: [{PackageFile}]", packageFilePath);

            using var outputStream = File.Open(packageFilePath, FileMode.OpenOrCreate, FileAccess.Write);

            await packageFileStream.CopyToAsync(outputStream, cancellationToken);

            await packageFileStream.FlushAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<string> WritePackageFileStreamAsync(int id,
                                                              string packageFileName,
                                                              Stream stream,
                                                              CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(packageFileName);

            if (_options.Value.PackagesPath.CreateDirectory())
            {
                _logger.LogInformation("Creating packages directory [{Folder}]", _options.Value.PackagesPath);
            }

            var packageFilePath = Path.Combine(_options.Value.PackagesPath, id.ToString());

            if (Directory.Exists(packageFilePath))
            {
                _logger.LogInformation("Emptying package directory [{Folder}]", packageFilePath);

                packageFilePath.EmptyDirectory();
            }
            else
            {
                _logger.LogInformation("Creating package directory [{Folder}]", packageFilePath);

                packageFilePath.CreateDirectory();
            }

            packageFilePath = Path.Combine(packageFilePath, packageFileName);

            _logger.LogInformation("Writing Package File: [{PackageFile}]", packageFilePath);

            using var outputStream = File.Open(packageFilePath, FileMode.OpenOrCreate, FileAccess.Write);

            await stream.CopyToAsync(outputStream, cancellationToken);

            await stream.FlushAsync(cancellationToken);

            return packageFilePath;
        }
    }
}