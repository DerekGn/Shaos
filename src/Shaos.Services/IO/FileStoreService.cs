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
        public Stream CreateAssemblyFileStream(
            string folder,
            string assemblyFileName,
            out string? assemblyFilePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(folder);
            ArgumentException.ThrowIfNullOrWhiteSpace(assemblyFileName);

            var assemblyStoreFolder = Path.Combine(_options.Value.AssemblyFilesPath, folder);

            if (!Directory.Exists(assemblyStoreFolder))
            {
                _logger.LogInformation("Creating folder: [{Folder}]", assemblyStoreFolder);

                Directory.CreateDirectory(assemblyStoreFolder);
            }

            assemblyFilePath = Path.Combine(assemblyStoreFolder, assemblyFileName);

            return File.OpenWrite(assemblyFilePath);
        }

        /// <inheritdoc/>
        public void DeleteCodeFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else
            {
                _logger.LogWarning("File: [{File}] Not Found", filePath);
            }
        }

        /// <inheritdoc/>
        public void DeleteCodeFolder(string folder)
        {
            if (!string.IsNullOrEmpty(folder))
            {
                var codeStoreFolder = Path.Combine(_options.Value.CodeFilesPath, folder);

                if (Directory.Exists(codeStoreFolder))
                {
                    _logger.LogInformation("Deleting Folder: [{Folder}]", codeStoreFolder);

                    Directory.Delete(codeStoreFolder, true);
                }
            }
            else
            {
                _logger.LogWarning("Folder: [{Folder}] Not Found", folder);
            }
        }

        /// <inheritdoc/>
        public Stream? GetCodeFileStream(string filePath)
        {
            Stream? result = null;

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                _logger.LogInformation("Opening File [{File}]", filePath);

                result = File.OpenRead(filePath);
            }
            else
            {
                _logger.LogWarning("File: [{File}] Not Found", filePath);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<string?> WriteCodeFileStreamAsync(
            string folder,
            string fileName,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            var codeStoreFolder = Path.Combine(_options.Value.CodeFilesPath, folder);

            if (!Directory.Exists(codeStoreFolder))
            {
                Directory.CreateDirectory(codeStoreFolder);
            }

            var codeFilePath = Path.Combine(codeStoreFolder, fileName);

            using var outputStream = File.Open(codeFilePath, FileMode.OpenOrCreate, FileAccess.Write);

            await stream.CopyToAsync(outputStream, cancellationToken);

            return codeFilePath;
        }
    }
}