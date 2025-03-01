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

using Microsoft.Extensions.Options;
using Shaos.Services.Options;

namespace Shaos.Services
{
    public class FileStoreService : IFileStoreService
    {
        private readonly IOptions<FileStoreOptions> _options;

        public FileStoreService(IOptions<FileStoreOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Stream CreateAssemblyFileStream(
            string folder,
            string assemblyFileName,
            out string? assemblyFilePath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(folder);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(assemblyFileName);

            var assemblyStoreFolder = Path.Combine(_options.Value.AssemblyFilesPath, folder);

            if (!Directory.Exists(assemblyStoreFolder))
            {
                Directory.CreateDirectory(assemblyStoreFolder);
            }

            assemblyFilePath = Path.Combine(assemblyStoreFolder, assemblyFileName);

            return File.OpenWrite(assemblyFilePath);
        }

        public void DeleteCodeFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void DeleteCodeFolder(string folder)
        {
            if (!string.IsNullOrEmpty(folder))
            {
                var codeStoreFolder = Path.Combine(_options.Value.CodeFilesPath, folder);

                if (Directory.Exists(codeStoreFolder))
                {
                    Directory.Delete(codeStoreFolder, true);
                }
            }
        }

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