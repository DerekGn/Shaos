using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shaos.Repository.Models;
using Shaos.Repository.Models.Devices.Parameters;
using System.Reflection.Metadata;

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

            builder
                .HasOne(_ => _.Parameter)
                .WithOne(_ => _.DashboardItem)
                .HasForeignKey<BaseParameter>(_ => _.DashboardItemId)
                .IsRequired(false);
        }
    }
}
