using LynxPmCore.Domain.Aggregates.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class ComponentUnitConfiguration : IEntityTypeConfiguration<ComponentUnit>
{
    public void Configure(EntityTypeBuilder<ComponentUnit> builder)
    {
        builder.ToTable("LYNX_PM_COMPONENTS_UNITS");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("ID");
        builder.Property(c => c.ComponentCode).HasColumnName("COMPONENT_CODE").HasMaxLength(25);
        builder.Property(c => c.Unit).HasColumnName("UNIT").HasMaxLength(5);
    }
}
