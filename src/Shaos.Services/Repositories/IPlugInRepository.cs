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
using Shaos.Services.Exceptions;
using System.Linq.Expressions;

namespace Shaos.Services.Repositories
{
    public interface IPlugInRepository : IBaseRepository
    {
        /// <summary>
        /// Create a new <see cref="PlugIn"/> instance
        /// </summary>
        /// <param name="plugIn">The <see cref="PlugIn"/> </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The identifier of the new <see cref="PlugIn"/></returns>
        /// <exception cref="ArgumentNullException">Thrown if a non null-able argument is null</exception>
        /// <exception cref="PlugInNameExistsException">Thrown if an existing <see cref="PlugIn"/> has the same name</exception>
        Task<int> CreateAsync(PlugIn plugIn,
                              CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new <see cref="Package"/> instance
        /// </summary>
        /// <param name="plugIn">The <see cref="PlugIn"/> instance to associate the <see cref="Package"/></param>
        /// <param name="package">The <see cref="Package"/> instance to associate the <see cref="PlugIn"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The <see cref="Package"/> identifier</returns>
        Task<int> CreatePackageAsync(PlugIn plugIn,
                                     Package package,
                                     CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a <see cref="PlugIn"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugIn"/> to delete</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The number of rows deleted</returns>
        Task<int> DeleteAsync(int id,
                              CancellationToken cancellationToken = default);

        /// <summary>
        /// Determine if a <see cref="PlugIn"/> with <paramref name="id"/> exists in the store
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugIn"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>True if the <see cref="PlugIn"/> exists</returns>
        Task<bool> ExistsAsync(int id,
                               CancellationToken cancellationToken = default);

        /// <summary>
        /// Get an <see cref="IAsyncEnumerable{T}"/> of <see cref="PlugIn"/> instances
        /// </summary>
        /// <param name="filter">The filter expression to filter the <see cref="PlugIn"/></param>
        /// <param name="orderBy">The order by function to apply to the <see cref="IQueryable{T}"/> of <see cref="PlugIn"/> instances</param>
        /// <param name="withNoTracking">Disables change tracking on the returned <see cref="PlugIn"/></param>
        /// <param name="includeProperties">The list of child properties to include in the query</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>A <see cref="IAsyncEnumerable{T}"/> of <see cref="PlugIn"/> instances</returns>
        IAsyncEnumerable<PlugIn> GetAsync(Expression<Func<PlugIn, bool>>? filter = null,
                                          Func<IQueryable<PlugIn>, IOrderedQueryable<PlugIn>>? orderBy = null,
                                          bool withNoTracking = true,
                                          List<string>? includeProperties = null,
                                          CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a <see cref="PlugIn"/> by identifier
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugIn"/></param>
        /// <param name="withNoTracking">Disables change tracking on the returned <see cref="PlugIn"/></param>
        /// <param name="includeProperties">The list of child properties to include in the query</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>A <see cref="PlugIn"/> instance if found</returns>
        Task<PlugIn?> GetByIdAsync(int id,
                                   bool withNoTracking = true,
                                   List<string>? includeProperties = null,
                                   CancellationToken cancellationToken = default);

        /// <summary>
        /// Get an <see cref="IQueryable{T}"/> of <see cref="PlugIn"/> instances
        /// </summary>
        /// <param name="filter">The filter expression to filter the <see cref="PlugIn"/></param>
        /// <param name="orderBy">The order by function to apply to the <see cref="IQueryable{T}"/> of <see cref="PlugIn"/> instances</param>
        /// <param name="withNoTracking">Disables change tracking on the returned <see cref="PlugIn"/></param>
        /// <param name="includeProperties">The list of child properties to include in the query</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>A <see cref="IQueryable{T}"/> of <see cref="PlugIn"/> instances</returns>
        IQueryable<PlugIn> GetQueryable(Expression<Func<PlugIn, bool>>? filter = null,
                                        Func<IQueryable<PlugIn>, IOrderedQueryable<PlugIn>>? orderBy = null,
                                        bool withNoTracking = true,
                                        List<string>? includeProperties = null);

        /// <summary>
        /// Update a <see cref="PlugIn"/> instance
        /// </summary>
        /// <param name="id">The <see cref="PlugIn"/> identifier</param>
        /// <param name="name">The <see cref="PlugIn"/> name</param>
        /// <param name="description">The <see cref="PlugIn"/> description</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The <see cref="PlugIn"/> if found and updates are applied</returns>
        /// <exception cref="ArgumentNullException">Thrown if a non null-able argument is null</exception>
        /// <exception cref="PlugInNameExistsException">Thrown if an existing <see cref="PlugIn"/> has the same name</exception>
        Task<PlugIn?> UpdateAsync(int id,
                                  string name,
                                  string description,
                                  CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a <see cref="Package"/> instance
        /// </summary>
        /// <param name="plugIn">The <see cref="PlugIn"/> instance to associate the <see cref="Package"/></param>
        /// <param name="fileName">The file name of the <see cref="Package"/></param>
        /// <param name="assemblyFile">The <see cref="PlugIn"/> assembly</param>
        /// <param name="version">The <see cref="Package"/> assembly version</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        Task UpdatePackageAsync(PlugIn plugIn,
                                string fileName,
                                string assemblyFile,
                                string version,
                                CancellationToken cancellationToken = default);
    }
}