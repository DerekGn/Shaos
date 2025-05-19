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

using Shaos.Repository.Models;

namespace Shaos.Services.IO
{
    /// <summary>
    /// Represents a file store
    /// </summary>
    public interface IFileStoreService
    {
        /// <summary>
        /// Delete a PlugIn package from the file store
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fileName"></param>
        void DeletePackage(int id, string fileName);

        /// <summary>
        /// Extract a package
        /// </summary>
        /// <param name="id"></param>
        /// <param name="packageFileName"></param>
        IEnumerable<string> ExtractPackage(int id, string packageFileName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetAssemblyPath(int id);

        /// <summary>
        /// </summary>
        /// <param name="id">The <see cref="PlugIn"/> identifier</param>
        /// <param name="packageFileName">The package filename to write too</param>
        /// <param name="stream">The stream to be written</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The fully qualified file path of the file written to the file store</returns>
        Task<string> WritePackageFileStreamAsync(
            int id,
            string packageFileName,
            Stream stream,
            CancellationToken cancellationToken = default);
    }
}