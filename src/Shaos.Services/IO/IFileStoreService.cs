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

namespace Shaos.Services.IO
{
    /// <summary>
    /// Represents a file store
    /// </summary>
    public interface IFileStoreService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="assemblyFileName"></param>
        /// <param name="assemblyFilePath"></param>
        /// <returns></returns>
        Stream CreateAssemblyFileStream(
            string folder,
            string assemblyFileName,
            out string? assemblyFilePath);

        /// <summary>
        /// Delete a CodeFile
        /// </summary>
        /// <param name="filePath">The file path of the file to delete</param>
        void DeleteCodeFile(string filePath);

        /// <summary>
        /// Delete a code folder
        /// </summary>
        /// <param name="folder">The folder to delete</param>
        void DeleteCodeFolder(string folder);

        /// <summary>
        /// Open a code file from the file store
        /// </summary>
        /// <param name="filePath">The file path to the code file</param>
        /// <returns>The open stream if the code file was found</returns>
        Stream? GetCodeFileStream(
            string filePath);

        /// <summary>
        /// Write the contents of a stream to a <paramref name="folder"/> <paramref name="fileName"/> combination
        /// </summary>
        /// <param name="folder">The folder to write the file too</param>
        /// <param name="fileName">The filename to write too</param>
        /// <param name="stream">The stream to be written to the <paramref name="folder"/> <paramref name="fileName"/> combination</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The fully qualified file path of the file written to the file store</returns>
        Task<string?> WriteCodeFileStreamAsync(
            string folder,
            string fileName,
            Stream stream,
            CancellationToken cancellationToken = default);
    }
}
