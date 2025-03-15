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
using System.Runtime.CompilerServices;

namespace Shaos.Services.Store
{
    /// <summary>
    /// The store interface definition
    /// </summary>
    public interface IStore
    {
        /// <summary>
        /// Create a new <see cref="PlugIn"/> instance
        /// </summary>
        /// <param name="name">The <see cref="PlugIn"/> name</param>
        /// <param name="description">The <see cref="PlugIn"/> description</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The identifier of the new <see cref="PlugIn"/></returns>
        Task<int> CreatePlugInAsync(
            string name,
            string? description,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new <see cref="PlugInInstance"/> instance
        /// </summary>
        /// <param name="name">The <see cref="PlugInInstance"/> name</param>
        /// <param name="description">The <see cref="PlugInInstance"/> description</param>
        /// <param name="plugIn">The <see cref="PlugIn"/> instance to associate the <see cref="PlugInInstance"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The <see cref="PlugInInstance"/> identifier</returns>
        Task<int> CreatePlugInInstanceAsync(
            string name,
            string description,
            PlugIn plugIn,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new <see cref="Package"/> instance
        /// </summary>
        /// <param name="plugIn">The <see cref="PlugIn"/> instance to associate the <see cref="Package"/></param>
        /// <param name="filename">The file name of the <see cref="Package"/></param>
        /// <param name="filePath">The fully qualified file path to the PlugIn assembly</param>
        /// <param name="version">The <see cref="Package"/> assembly version</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The <see cref="Package"/> identifier</returns>
        Task<int> CreatePlugInPackageAsync(
            PlugIn plugIn,
            string fileName,
            string filePath,
            string version,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a <see cref="BaseEntity"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="BaseEntity"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        Task DeleteAsync<T>(int id, CancellationToken cancellationToken = default) where T : BaseEntity;

        /// <summary>
        /// Get a <see cref="PlugIn"/> instance by identifier
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugIn"/></param>
        /// <param name="withNoTracking">Indicates if the <see cref="PlugIn"/> is tracked by the context</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The <see cref="PlugIn"/> if found</returns>
        Task<PlugIn?> GetPlugInByIdAsync(
            int id,
            bool withNoTracking = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a <see cref="PlugIn"/> instance by its name
        /// </summary>
        /// <param name="name">The name of the <see cref="PlugIn"/> to search for</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The <see cref="PlugIn"/> if found</returns>
        Task<PlugIn?> GetPlugInByNameAsync(
            string name,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a <see cref="PlugInInstance"/> by its identifier
        /// </summary>
        /// <param name="id">The <see cref="PlugInInstance"/> identifier</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The <see cref="PlugInInstance"/> if found</returns>
        Task<PlugInInstance?> GetPlugInInstanceByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a <see cref="PlugInInstance"/> instance by its name
        /// </summary>
        /// <param name="name">The name of the <see cref="PlugInInstance"/> to search for</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The <see cref="PlugInInstance"/> if found</returns>
        Task<PlugInInstance?> GetPlugInInstanceByNameAsync(
            string name,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the set of <see cref="PlugIn"/>
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="PlugIn"/></returns>
        IAsyncEnumerable<PlugIn> GetPlugInsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default);

        /// <summary>
        /// Save changes to the context
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a <see cref="PlugIn"/> instance
        /// </summary>
        /// <param name="id">The <see cref="PlugIn"/> identifier</param>
        /// <param name="name">The <see cref="PlugIn"/> name</param>
        /// <param name="description">The <see cref="PlugIn"/> description</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The <see cref="PlugIn"/> if found and updates are applied</returns>
        Task<PlugIn?> UpdatePlugInAsync(
            int id,
            string name,
            string? description,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plugIn">The <see cref="PlugIn"/> instance to associate the <see cref="Package"/></param>
        /// <param name="filename">The file name of the <see cref="Package"/></param>
        /// <param name="filePath">The fully qualified file path to the PlugIn assembly</param>
        /// <param name="version">The <see cref="Package"/> assembly version</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns></returns>
        Task UpdatePlugInPackageAsync(
            PlugIn plugIn,
            string fileName,
            string filePath,
            string version,
            CancellationToken cancellationToken = default);
    }
}