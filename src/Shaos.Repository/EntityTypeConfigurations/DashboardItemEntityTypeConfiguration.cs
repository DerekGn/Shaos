using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shaos.Repository.Models;

namespace Shaos.Repository.EntityTypeConfigurations
{
    /// <summary>
    /// The <see cref="DashboardItem"/> EF configuration
    /// </summary>
    public class DashboardItemEntityTypeConfiguration : IEntityTypeConfiguration<DashboardItem>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<DashboardItem> builder)
        {
            builder
                .Property(_ => _.Id);

            builder
                .Property(_ => _.Label)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
