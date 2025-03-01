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

namespace Shaos.Services
{
    public interface IPlugInService
    {
        Task<int> CreatePlugInAsync(
            string name,
            string? description,
            CancellationToken cancellationToken = default);

        Task<int> CreatePlugInInstanceAsync(
            int id,
            CreatePlugInInstance create,
            CancellationToken cancellationToken = default);

        Task DeletePlugInAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task DeletePlugInCodeFileAsync(
            int id,
            int codeFileId,
            CancellationToken cancellationToken = default);

        Task DeletePlugInInstanceAsync(
            int id,
            int instanceId,
            CancellationToken cancellationToken = default);

        Task<PlugIn?> GetPlugInByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<PlugIn?> GetPlugInByNameAsync(
            string name,
            CancellationToken cancellationToken = default);

        IAsyncEnumerable<PlugIn> GetPlugInsAsync(
            CancellationToken cancellationToken = default);

        Task SetPlugInInstanceEnableAsync(
            int id,
            bool enable,
            CancellationToken cancellationToken = default);

        Task<PlugIn?> UpdatePlugInAsync(
            int id,
            UpdatePlugIn update,
            CancellationToken cancellationToken = default);

        Task UpdatePlugInInstanceAsync(
            int id,
            UpdatePlugInInstance update,
            CancellationToken cancellationToken = default);

        Task UploadPlugInCodeFileAsync(
            int id,
            string fileName,
            Stream stream,
            CancellationToken cancellationToken = default);
    }
}