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

using Serilog.Events;
using Shaos.Repository.Exceptions;
using Shaos.Repository.Models;
using System.Linq.Expressions;

namespace Shaos.Repository
{
    /// <summary>
    /// Defines a <see cref="IShaosRepository"/> interface
    /// </summary>
    public interface IShaosRepository
    {
        /// <summary>
        /// Determine if a <see cref="BaseEntity"/> exists in the store based on <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>True if the <see cref="BaseEntity"/> exists</returns>
        Task<bool> AnyAsync<T>(Expression<Func<T, bool>>? predicate,
                               CancellationToken cancellationToken = default) where T : BaseEntity;

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
        /// Create a new <see cref="PlugIn"/> instance
        /// </summary>
        /// <param name="plugIn">The <see cref="PlugIn"/> </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The identifier of the new <see cref="PlugIn"/></returns>
        /// <exception cref="ArgumentNullException">Thrown if a non null-able argument is null</exception>
        /// <exception cref="NameExistsException">Thrown if an existing <see cref="PlugIn"/> has the same name</exception>
        Task<int> CreatePlugInAsync(PlugIn plugIn,
                                    CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new <see cref="PlugInInstance"/> instance
        /// </summary>
        /// <param name="plugIn">The <see cref="PlugIn"/> instance to associate to the <see cref="PlugInInstance"/></param>
        /// <param name="plugInInstance">The <see cref="PlugInInstance"/> instance to associate with the <see cref="PlugIn"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The <see cref="PlugInInstance"/> identifier</returns>
        /// <exception cref="ArgumentNullException">Thrown if a non null-able argument is null</exception>
        /// <exception cref="NameExistsException">Thrown if an existing <see cref="PlugInInstance"/> has the same name</exception>
        Task<int> CreatePlugInInstanceAsync(PlugIn plugIn,
                                            PlugInInstance plugInInstance,
                                            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a <see cref="BaseEntity"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="BaseEntity"/> to delete</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The number of rows deleted</returns>
        Task DeleteAsync<T>(int id,
                            CancellationToken cancellationToken = default) where T : BaseEntity;

        /// <summary>
        /// Get a <see cref="BaseEntity"/> instance by identifier
        /// </summary>
        /// <param name="id">The identifier of the <see cref="BaseEntity"/></param>
        /// <param name="withNoTracking">Disables change tracking on the returned <see cref="BaseEntity"/></param>
        /// <param name="includeProperties">The list of child properties to include in the query</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>A <see cref="PlugIn"/> instance if found</returns>
        Task<T?> GetByIdAsync<T>(int id,
                                 bool withNoTracking = true,
                                 List<string>? includeProperties = null,
                                 CancellationToken cancellationToken = default) where T : BaseEntity;

        /// <summary>
        /// Get the <see cref="LogLevelSwitch"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>A <see cref="LogLevelSwitch"/></returns>
        Task<LogLevelSwitch?> GetByNameAsync(string name,
                                             CancellationToken cancellationToken = default);

        /// <summary>
        /// Get an <see cref="IAsyncEnumerable{T}"/> of <see cref="BaseEntity"/> instances
        /// </summary>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <param name="orderBy">The order by function to apply to the <see cref="IQueryable{T}"/> of <see cref="BaseEntity"/> instances</param>
        /// <param name="withNoTracking">Disables change tracking on the returned <see cref="BaseEntity"/></param>
        /// <param name="includeProperties">The list of child properties to include in the query</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>A <see cref="IAsyncEnumerable{T}"/> of <see cref="BaseEntity"/> instances</returns>
        IAsyncEnumerable<T> GetEnumerableAsync<T>(Expression<Func<T, bool>>? predicate = null,
                                                  Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                                  bool withNoTracking = true,
                                                  List<string>? includeProperties = null,
                                                  CancellationToken cancellationToken = default) where T : BaseEntity;

        /// <summary>
        /// Get the <see cref="BaseEntity"/>
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> instance</returns>
        IAsyncEnumerable<T> GetEnumerableAsync<T>(CancellationToken cancellationToken = default) where T : BaseEntity;

        /// <summary>
        /// Get the first or default value matching the <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="T">The <see cref="BaseEntity"/></typeparam>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>An instance of a <see cref="BaseEntity"/></returns>
        Task<T?> GetFirstOrDefaultAsync<T>(Expression<Func<T, bool>>? predicate = null,
                                           CancellationToken cancellationToken = default) where T : BaseEntity;

        /// <summary>
        /// Get an <see cref="IQueryable{T}"/> of <see cref="BaseEntity"/> instances
        /// </summary>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <param name="orderBy">The order by function to apply to the <see cref="IQueryable{T}"/> of <see cref="BaseEntity"/> instances</param>
        /// <param name="withNoTracking">Disables change tracking on the returned <see cref="BaseEntity"/></param>
        /// <param name="includeProperties">The list of child properties to include in the query</param>
        /// <returns>A <see cref="IQueryable{T}"/> of <see cref="BaseEntity"/> instances</returns>
        IQueryable<T> GetQueryable<T>(Expression<Func<T, bool>>? predicate = null,
                                      Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                      bool withNoTracking = true,
                                      List<string>? includeProperties = null) where T : BaseEntity;

        /// <summary>
        /// Saves changes to the context
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The number of changes mad to the database</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a <see cref="PlugIn"/> instance
        /// </summary>
        /// <param name="id">The <see cref="PlugIn"/> identifier</param>
        /// <param name="name">The <see cref="PlugIn"/> name</param>
        /// <param name="description">The <see cref="PlugIn"/> description</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The <see cref="PlugIn"/> if found and updates are applied</returns>
        /// <exception cref="ArgumentNullException">Thrown if a non null-able argument is null</exception>
        /// <exception cref="NameExistsException">Thrown if an existing <see cref="PlugIn"/> has the same name</exception>
        Task<PlugIn?> UpdatePlugInAsync(int id,
                                        string name,
                                        string description,
                                        CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a <see cref="PlugInInstance"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugInInstance"/></param>
        /// <param name="enabled">The enabled state of the <see cref="PlugInInstance"/></param>
        /// <param name="name">The updated name for the <see cref="PlugInInstance"/></param>
        /// <param name="description">The updated description of the <see cref="PlugInInstance"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <exception cref="ArgumentNullException">Thrown if a non null-able argument is null</exception>
        /// <exception cref="NameExistsException">Thrown if an existing <see cref="PlugInInstance"/> has the same name</exception>
        Task UpdatePlugInInstanceAsync(int id,
                                       bool? enabled = default,
                                       string? name = default,
                                       string? description = default,
                                       CancellationToken cancellationToken = default);

        /// <summary>
        /// Upsert a <see cref="LogLevelSwitch"/>
        /// </summary>
        /// <param name="name">The name of the <see cref="LogLevelSwitch"/> to upsert</param>
        /// <param name="level">The <see cref="LogEventLevel"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The upserted <see cref="LogLevelSwitch"/></returns>
        Task<LogLevelSwitch> UpsertLogLevelSwitchAsync(string name,
                                                       LogEventLevel level,
                                                       CancellationToken cancellationToken = default);
    }
}