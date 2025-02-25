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

namespace Shaos.Services
{
    public class FileStoreService : IFileStoreService
    {
        private readonly IOptions<FileStoreOptions> _options;

        public FileStoreService(IOptions<FileStoreOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void DeleteFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void DeleteFolder(string folder)
        {
            if(!string.IsNullOrEmpty(folder))
            {
                var storeFolder = Path.Combine(_options.Value.StorePath, folder);

                if (!Directory.Exists(storeFolder))
                {
                    Directory.Delete(storeFolder);
                }
            }
        }

        public async Task<string?> WriteFileStreamAsync(
            string folder,
            string fileName,
            Stream stream,
            CancellationToken cancellationToken)
        {
            var storeFolder = Path.Combine(_options.Value.StorePath, folder);

            if (!Directory.Exists(storeFolder))
            {
                Directory.CreateDirectory(storeFolder);
            }

            var filePath = Path.Combine(storeFolder, fileName);

            using var outputStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write);

            await stream.CopyToAsync(outputStream, cancellationToken);

            return filePath;
        }
    }
}
