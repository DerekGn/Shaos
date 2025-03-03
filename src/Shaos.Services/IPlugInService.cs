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
        /// <summary>
        /// Creates a new <see cref="PlugIn"/>
        /// </summary>
        /// <param name="createPlugIn">The plug in create attributes</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        /// <returns>The identifier of the created <see cref="PlugIn"/></returns>
        Task<int> CreatePlugInAsync(
            CreatePlugIn createPlugIn,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new <see cref="CodeFile"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugIn"/></param>
        /// <param name="fileName">The file name for the <see cref="PlugIn"/></param>
        /// <param name="stream">The <see cref="Stream"/> to write to the <paramref name="fileName"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        /// <returns>The identifier of the created <see cref="CodeFile"/></returns>
        Task CreatePlugInCodeFileAsync(
            int id,
            string fileName,
            Stream stream,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create an instance of a <see cref="PlugIn"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugIn"/> to create the instance</param>
        /// <param name="create">The create instance attributes</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        /// <returns>The identifier of the created <see cref="PlugInInstance"/></returns>
        Task<int> CreatePlugInInstanceAsync(
            int id,
            CreatePlugInInstance create,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a <see cref="PlugIn"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugIn"/> to delete</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        Task DeletePlugInAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a <see cref="CodeFile"/> from a <see cref="PlugIn"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugIn"/></param>
        /// <param name="codeFileId">The identifier of the <see cref="CodeFile"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        Task DeletePlugInCodeFileAsync(
            int id,
            int codeFileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a <see cref="PlugInInstance"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugInInstance"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        /// <returns></returns>
        Task DeletePlugInInstanceAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a <see cref="PlugIn"/> instance by its identifier
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugIn"/> to retrieve</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        /// <returns>The <see cref="PlugIn"/> instance if found</returns>
        Task<PlugIn?> GetPlugInByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a <see cref="PlugIn"/> instance by its name
        /// </summary>
        /// <param name="name">The name of the <see cref="PlugIn"/> to search for</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        /// <returns>The <see cref="PlugIn"/> if found</returns>
        Task<PlugIn?> GetPlugInByNameAsync(
            string name,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a <see cref="Stream"/> to a <see cref="CodeFile"/> file
        /// </summary>
        /// <param name="id">The <see cref="CodeFile"/> identifier</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        /// <returns>The <see cref="Stream"/> if the code file exists</returns>
        Task<Stream?> GetPlugInCodeFileStreamAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a <see cref="PlugInInstance"/> by its identifier
        /// </summary>
        /// <param name="id">The <see cref="PlugInInstance"/> identifier</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        /// <returns></returns>
        Task<PlugInInstance?> GetPlugInInstanceByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a <see cref="PlugInInstance"/> instance by its name
        /// </summary>
        /// <param name="name">The name of the <see cref="PlugInInstance"/> to search for</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        /// <returns>The <see cref="PlugInInstance"/> if found</returns>
        Task<PlugInInstance?> GetPlugInInstanceByNameAsync(
            string name,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the set of <see cref="PlugIn"/>
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="PlugIn"/></returns>
        IAsyncEnumerable<PlugIn> GetPlugInsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Set the <paramref name="enable"/> state of a <see cref="PlugIn"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugInInstance"/></param>
        /// <param name="enable">The enable state to set</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        Task SetPlugInInstanceEnableAsync(
            int id,
            bool enable,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the properties of a <see cref="PlugIn"/>
        /// </summary>
        /// <param name="id">The <see cref="PlugIn"/> identifier</param>
        /// <param name="update">The <see cref="UpdatePlugIn"/> properties to update</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        /// <returns>The <see cref="PlugIn"/> instance if found</returns>
        Task<PlugIn?> UpdatePlugInAsync(
            int id,
            UpdatePlugIn update,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the properties of a <see cref="PlugInInstance"/>
        /// </summary>
        /// <param name="id">The <see cref="PlugInInstance"/> identifier</param>
        /// <param name="update">The <see cref="UpdatePlugInInstance"/> properties to update</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        /// <returns></returns>
        Task UpdatePlugInInstanceAsync(
            int id,
            UpdatePlugInInstance update,
            CancellationToken cancellationToken = default);
    }
}