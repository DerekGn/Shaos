using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shaos.Repository.Models;

namespace Shaos.Repository
{
    public interface IDbContext
    {
        /// <summary>
        /// Remove an entity from the data base context
        /// </summary>
        /// <typeparam name="TEntity">The entity type to remove</typeparam>
        /// <param name="entity">The entity to remove</param>
        /// <returns>The removed entity</returns>
        EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// The <see cref="DbSet{TEntity}"/> of <see cref="CodeFile"/>
        /// </summary>
        DbSet<CodeFile> CodeFiles { get; set; }

        /// <summary>
        /// The <see cref="DbSet{TEntity}"/> of <see cref="PlugInInstance"/>
        /// </summary>
        DbSet<PlugInInstance> PlugInInstances { get; set; }

        /// <summary>
        /// The <see cref="DbSet{TEntity}"/> of <see cref="PlugIn"/>
        /// </summary>
        DbSet<PlugIn> PlugIns { get; set; }

        /// <summary>
        /// Save changes applied to the context
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task that represents the asynchronous save operation.
        /// The task result contains the number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}