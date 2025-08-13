
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shaos.Repository.Models.Devices;

namespace Shaos.Repository.EntityTypeConfigurations
{
    /// <summary>
    /// The <see cref="DeviceUpdate"/> EF configuration
    /// </summary>
    public class DeviceUpdateEntityTypeConfiguration : IEntityTypeConfiguration<DeviceUpdate>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<DeviceUpdate> builder)
        {
            builder
                .Property(_ => _.Id);
        }
    }
}
