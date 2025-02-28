using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shaos.Repository;
using Shaos.Repository.Models;

namespace Shaos.Services
{
    public abstract class BasePlugInService
    {
        protected readonly ShaosDbContext Context;
        protected readonly ILogger<BasePlugInService> Logger;

        protected BasePlugInService(
            ILogger<BasePlugInService> logger,
            ShaosDbContext context)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        internal async Task<PlugIn?> GetPlugInByIdFromContextAsync(
            int id,
            bool withNoTracking = true,
            CancellationToken cancellationToken = default)
        {
            var query = Context.PlugIns.Include(_ => _.CodeFiles).AsQueryable();

            if (withNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(_ => _.Id == id, cancellationToken);
        }
    }
}