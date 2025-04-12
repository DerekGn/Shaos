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
    public interface IPlugInInstanceRepository : IBaseRepository
    {
        /// <summary>
        /// Create a new <see cref="PlugInInstance"/> instance
        /// </summary>
        /// <param name="plugIn">The <see cref="PlugIn"/> instance to associate to the <see cref="PlugInInstance"/></param>
        /// <param name="plugInInstance">The <see cref="PlugInInstance"/> instance to associate with the <see cref="PlugIn"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The <see cref="PlugInInstance"/> identifier</returns>
        /// <exception cref="ArgumentNullException">Thrown if a non null-able argument is null</exception>
        /// <exception cref="PlugInInstanceNameExistsException">Thrown if an existing <see cref="PlugInInstance"/> has the same name</exception>
        Task<int> CreatePlugInInstanceAsync(
            PlugIn plugIn,
            PlugInInstance plugInInstance,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a <see cref="PlugInInstance"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugInInstance"/> to delete</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The number of rows deleted</returns>
        Task DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="withNoTracking"></param>
        /// <param name="includeProperties"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerable<PlugInInstance> GetAsync(
            Expression<Func<PlugInInstance, bool>>? filter = null,
            Func<IQueryable<PlugInInstance>, IOrderedQueryable<PlugInInstance>>? orderBy = null,
            bool withNoTracking = true,
            List<string>? includeProperties = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a <see cref="PlugInInstance"/> by identifier
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugInInstance"/></param>
        /// <param name="withNoTracking">Disables change tracking on the returned <see cref="PlugInInstance"/></param>
        /// <param name="includeProperties">The list of child properties to include in the query</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>A <see cref="PlugInInstance"/> instance if found</returns>
        Task<PlugInInstance?> GetByIdAsync(
            int id,
            bool withNoTracking = true,
            List<string>? includeProperties = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a <see cref="PlugInInstance"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugInInstance"/></param>
        /// <param name="name">The updated name for the <see cref="PlugInInstance"/></param>
        /// <param name="description">The updated description of the <see cref="PlugInInstance"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <exception cref="ArgumentNullException">Thrown if a non null-able argument is null</exception>
        /// <exception cref="PlugInInstanceNameExistsException">Thrown if an existing <see cref="PlugInInstance"/> has the same name</exception>
        Task UpdatePlugInInstanceAsync(
            int id,
            string name,
            string description,
            CancellationToken cancellationToken = default);
    }
}