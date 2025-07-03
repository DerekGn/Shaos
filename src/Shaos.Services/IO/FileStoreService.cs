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
    /// A file store implementation
    /// </summary>
    public class FileStoreService : IFileStoreService
    {
        private readonly ILogger<FileStoreService> _logger;
        private readonly IOptions<FileStoreOptions> _options;

        /// <summary>
        /// Create an instance of a <see cref="IFileStoreService"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance</param>
        /// <param name="options">The <see cref="IOptions{TOptions}"/> instance</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FileStoreService(ILogger<FileStoreService> logger,
                                IOptions<FileStoreOptions> options)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(options);

            _logger = logger;
            _options = options;
        }

        /// <inheritdoc/>
        public void DeletePackage(string packageFileName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(packageFileName);

            var filePath = Path.Combine(_options.Value.PackagesPath, packageFileName);

            if (File.Exists(filePath))
            {
                _logger.LogInformation("Deleting file [{Path}]", filePath);

                File.Delete(filePath);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> ExtractPackage(string folder,
                                                  string packageFileName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(folder);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(packageFileName);

            var sourcePath = _options.Value.PackagesPath;
            var targetPath = Path.Combine(_options.Value.BinariesPath, folder);

            sourcePath = Path.Combine(sourcePath, packageFileName);

            _logger.LogInformation("Extracting package: [{SourcePath}] to [{TargetPath}]",
                sourcePath,
                targetPath);

            ZipFile.ExtractToDirectory(sourcePath, targetPath, true);

            return Directory.EnumerateFiles(targetPath);
        }

        /// <inheritdoc/>
        public string ExtractPackage(string folder,
                                     string packageFileName,
                                     out IEnumerable<string> files)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(folder);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(packageFileName);

            var sourcePath = _options.Value.PackagesPath;
            var targetPath = Path.Combine(_options.Value.BinariesPath, folder);

            sourcePath = Path.Combine(sourcePath, packageFileName);

            _logger.LogInformation("Extracting package: [{SourcePath}] to [{TargetPath}]",
                sourcePath,
                targetPath);

            ZipFile.ExtractToDirectory(sourcePath, targetPath, true);

            files = Directory.EnumerateFiles(targetPath);

            return targetPath;
        }

        /// <inheritdoc/>
        public string GetAssemblyPath(int id, string assemblyFileName)
        {
            return Path.Combine(Path.Combine(_options.Value.BinariesPath, id.ToString()), assemblyFileName);
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
    }
}