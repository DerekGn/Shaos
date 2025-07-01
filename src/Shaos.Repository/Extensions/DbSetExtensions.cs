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

using Microsoft.EntityFrameworkCore;
using Shaos.Repository.Models;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Shaos.Repository.Extensions
{
    /// <summary>
    /// A see <see cref="DbSet{TEntity}"/> extension
    /// </summary>
    public static class DbSetExtensions
    {
        /// <summary>
        /// Delete an entity from a <see cref="DbSet{TEntity}"/>
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="set">The <see cref="DbSet{TEntity}"/> to delete the entity from</param>
        /// <param name="id">The identifier of the entity to delete</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The number of rows deleted</returns>
        public static Task<int> DeleteAsync<T>(this DbSet<T> set,
                                               int id,
                                               CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return set.Where(_ => _.Id == id).ExecuteDeleteAsync(cancellationToken);
        }

        /// <summary>
        /// Gets a <see cref="IAsyncEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="set">The <see cref="DbSet{TEntity}"/> to delete the entity from</param>
        /// <param name="withNoTracking">Indicates if the set of entities are tracked by the context</param>
        /// <param name="filter">The filter expression to apply to the set</param>
        /// <param name="orderBy">The order expression to apply to the set</param>
        /// <param name="includeProperties">The set of include properties</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The <see cref="IAsyncEnumerable{T}"/></returns>
        public static async IAsyncEnumerable<T> GetAsync<T>(this DbSet<T> set,
                                                            bool withNoTracking = true,
                                                            Expression<Func<T, bool>>? filter = null,
                                                            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                                            List<string>? includeProperties = null,
                                                            [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : BaseEntity
        {
            IQueryable<T> query = set;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            if (withNoTracking)
            {
                query = query.AsNoTracking();
            }

            IAsyncEnumerable<T> enumerable;

            if (orderBy != null)
            {
                enumerable = orderBy(query).AsAsyncEnumerable();
            }
            else
            {
                enumerable = query.AsAsyncEnumerable();
            }

            await foreach (var item in enumerable.WithCancellation(cancellationToken))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Gets a null-able instance of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="set">The <see cref="DbSet{TEntity}"/> to delete the entity from</param>
        /// <param name="id">The identity of the entity to retrieve</param>
        /// <param name="withNoTracking">Indicates if the set of entities are tracked by the context</param>
        /// <param name="includeProperties">The set of include properties</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>An instance of <typeparamref name="T"/></returns>
        public static async Task<T?> GetByIdAsync<T>(this DbSet<T> set,
                                                     int id,
                                                     bool withNoTracking = true,
                                                     List<string>? includeProperties = null,
                                                     CancellationToken cancellationToken = default) where T : BaseEntity
        {
            IQueryable<T> query = set.Where(_ => _.Id == id);

            if (withNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Gets a <see cref="IQueryable{T}"/> of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="set">The <see cref="DbSet{TEntity}"/> to delete the entity from</param>
        /// <param name="withNoTracking">Indicates if the set of entities are tracked by the context</param>
        /// <param name="filter">The filter expression to apply to the set</param>
        /// <param name="orderBy">The order expression to apply to the set</param>
        /// <param name="includeProperties">The set of include properties</param>
        /// <returns>An <see cref="IQueryable{T}"/> of <typeparamref name="T"/></returns>
        public static IQueryable<T> GetQueryable<T>(this DbSet<T> set,
                                                    bool withNoTracking = true,
                                                    Expression<Func<T, bool>>? filter = null,
                                                    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                                    List<string>? includeProperties = null) where T : BaseEntity
        {
            IQueryable<T> query = set;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            if (withNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return query;
        }
    }
}
