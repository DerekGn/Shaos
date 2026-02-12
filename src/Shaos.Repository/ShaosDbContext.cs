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
using Shaos.Repository.EntityTypeConfigurations;
using Shaos.Repository.Models;
using Shaos.Repository.Models.Devices;

namespace Shaos.Repository
{
    /// <summary>
    /// The application store database context
    /// </summary>
    public class ShaosDbContext : DbContext
    {
        /// <inheritdoc/>
        public ShaosDbContext(DbContextOptions<ShaosDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// The set of <see cref="Device"/>
        /// </summary>
        public DbSet<Device> Devices { get; set; }

        /// <summary>
        /// The set of <see cref="LogLevelSwitch"/>
        /// </summary>
        public DbSet<LogLevelSwitch> LogLevelSwitches { get; set; }

        /// <summary>
        /// The set of <see cref="DashboardConfiguration"/>
        /// </summary>
        public DbSet<DashboardConfiguration> DashboardConfigurations { get; set; }

        /// <summary>
        /// The set of <see cref="PlugInInformation"/>
        /// </summary>
        public DbSet<PlugInInformation> PlugInInformations { get; set; }

        /// <summary>
        /// The set of <see cref="PlugInInstance"/>
        /// </summary>
        public DbSet<PlugInInstance> PlugInInstances { get; set; }

        /// <summary>
        /// The set of <see cref="PlugIn"/>
        /// </summary>
        public DbSet<PlugIn> PlugIns { get; set; }

        /// <inheritdoc/>>
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ChangeTracker.Entries<BaseEntity>().Where(_ => _.State == EntityState.Added).ToList().ForEach(_ =>
            {
                _.Entity.CreatedDate = DateTime.UtcNow;
            });

            ChangeTracker.Entries<BaseEntity>().Where(_ => _.State == EntityState.Modified).ToList().ForEach(_ =>
            {
                _.Entity.UpdatedDate = DateTime.UtcNow;
            });

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <inheritdoc/>>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlugInEntityTypeConfiguration).Assembly);
        }
    }
}