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

namespace Shaos.Repository
{
    /// <summary>
    /// The application store database context
    /// </summary>
    public class ShaosDbContext : DbContext
    {
        public ShaosDbContext(DbContextOptions<ShaosDbContext> options)
            : base(options)
        {
        }

        /// <inheritdoc/>>
        public DbSet<PlugIn> PlugIns { get; set; }

        /// <inheritdoc/>>
        public DbSet<NuGetPackage> NuGetPackages { get; set; }

        /// <inheritdoc/>>
        public DbSet<PlugInInstance> PlugInInstances { get; set; }

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
            modelBuilder.Entity<PlugIn>()
                .HasKey(_ => _.Id)
                .HasName("PrimaryKey_PlugInId");

            modelBuilder
                .Entity<PlugIn>()
                .HasOne(_ => _.NuGetPackage)
                .WithOne(_ => _.PlugIn)
                .HasForeignKey<NuGetPackage>(_ => _.PlugInId)
                .IsRequired(false);

            modelBuilder
               .Entity<PlugIn>()
               .HasMany(_ => _.Instances)
               .WithOne(_ => _.PlugIn)
               .HasForeignKey(_ => _.PlugInId)
               .IsRequired(false);

            modelBuilder
                .Entity<PlugIn>()
                .Property(_ => _.Name)
                .IsRequired()
                .HasMaxLength(ModelConstants.MaxNameLength);

            modelBuilder
                .Entity<PlugIn>()
                .Property(_ => _.Description)
                .HasMaxLength(ModelConstants.MaxDescriptionLength);

            modelBuilder
                .Entity<PlugIn>()
                .HasIndex(_ => _.Name )
                .HasDatabaseName("IX_PlugIn_Name_Ascending");

            modelBuilder.Entity<NuGetPackage>()
                .HasKey(_ => _.Id)
                .HasName("PrimaryKey_NuGetId");

            modelBuilder
                .Entity<NuGetPackage>()
                .Property(_ => _.FileName)
                .HasMaxLength(ModelConstants.MaxFileNameLength)
                .IsRequired();

#warning TODO
            modelBuilder
                .Entity<NuGetPackage>()
                .Property(_ => _.Version)
                .HasMaxLength(10)
                .IsRequired();

            modelBuilder.Entity<PlugInInstance>()
                .HasKey(_ => _.Id)
                .HasName("PrimaryKey_PlugInInstanceId");

            modelBuilder
                .Entity<PlugInInstance>()
                .Property(_ => _.Name)
                .IsRequired()
                .HasMaxLength(ModelConstants.MaxNameLength);

            modelBuilder
               .Entity<PlugInInstance>()
               .HasIndex(_ => _.Name)
               .HasDatabaseName("IX_PlugInInstance_Name_Ascending");
        }
    }
}