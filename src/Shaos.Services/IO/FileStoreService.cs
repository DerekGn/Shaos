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
    ///
    /// </summary>
    public class FileStoreService : IFileStoreService
    {
        private readonly ILogger<FileStoreService> _logger;
        private readonly IOptions<FileStoreOptions> _options;

        /// <summary>
        ///
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public FileStoreService(
            ILogger<FileStoreService> logger,
            IOptions<FileStoreOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public void DeletePackage(int id,
                                  string packageFileName)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

            var filePath = Path.Combine(Path.Combine(_options.Value.PackagesPath, id.ToString()), packageFileName);

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

            var sourcePath = Path.Combine(_options.Value.PackagesPath, folder);
            var targetPath = Path.Combine(_options.Value.BinariesPath, folder);

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
        public string GetAssemblyPath(int id, string assemblyFileName)
        {
            return Path.Combine(Path.Combine(_options.Value.BinariesPath, id.ToString()), assemblyFileName);
        }

        /// <inheritdoc/>
        public async Task<string> WritePackageFileStreamAsync(int id,
                                                              string packageFileName,
                                                              Stream packageFileStream,
                                                              CancellationToken cancellationToken = default)
        {
            return await WritePackageFileStreamAsync(id.ToString(), packageFileName, packageFileStream, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<string> WritePackageFileStreamAsync(string subFolder,
                                                              string packageFileName,
                                                              Stream packageFileStream,
                                                              CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(packageFileStream);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(subFolder);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(packageFileName);

            if (_options.Value.PackagesPath.CreateDirectory())
            {
                _logger.LogInformation("Creating packages directory [{Folder}]", _options.Value.PackagesPath);
            }

            var packageFilePath = Path.Combine(_options.Value.PackagesPath, subFolder);

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

            return packageFilePath;
        }
    }
}