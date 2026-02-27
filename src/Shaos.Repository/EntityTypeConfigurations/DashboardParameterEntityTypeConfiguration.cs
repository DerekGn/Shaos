using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shaos.Repository.Extensions;
using Shaos.Repository.Models;

namespace Shaos.Repository.EntityTypeConfigurations
{
    /// <summary>
    /// The <see cref="DashboardParameter"/> EF configuration
    /// </summary>
    public class DashboardParameterEntityTypeConfiguration : IEntityTypeConfiguration<DashboardParameter>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<DashboardParameter> builder)
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
