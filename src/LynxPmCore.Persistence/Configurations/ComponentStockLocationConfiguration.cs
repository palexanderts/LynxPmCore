using LynxPmCore.Domain.Aggregates.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class ComponentStockLocationConfiguration : IEntityTypeConfiguration<ComponentStockLocation>
{
    public void Configure(EntityTypeBuilder<ComponentStockLocation> builder)
    {
        builder.ToTable("LYNX_PM_COMPONENT_CENTER_STORE");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("ID");
        builder.Property(c => c.ComponentCode).HasColumnName("COMPONENT_CODE").HasMaxLength(25);
        builder.Property(c => c.Center).HasColumnName("CENTER").HasMaxLength(5);
        builder.Property(c => c.Store).HasColumnName("STORE").HasMaxLength(5);
        builder.Property(c => c.Location).HasColumnName("LOCATION").HasMaxLength(18);
        builder.Property(c => c.Quantity).HasColumnName("QUANTITY");
    }
}
