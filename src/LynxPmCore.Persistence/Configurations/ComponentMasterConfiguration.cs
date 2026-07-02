using LynxPmCore.Domain.Aggregates.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class ComponentMasterConfiguration : IEntityTypeConfiguration<ComponentMaster>
{
    public void Configure(EntityTypeBuilder<ComponentMaster> builder)
    {
        builder.ToTable("LYNX_PM_COMPONENTS");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("ID");
        builder.Property(c => c.Code).HasColumnName("CODE").HasMaxLength(25);
        builder.Property(c => c.Name).HasColumnName("NAME").HasMaxLength(50);
        builder.Property(c => c.Type).HasColumnName("TYPE").HasMaxLength(5);
        builder.Property(c => c.UnitBase).HasColumnName("UNIT_BASE").HasMaxLength(5);
        builder.Property(c => c.ManufacturePart).HasColumnName("MANUFACTURE_PART").HasMaxLength(25);
        builder.Property(c => c.Location).HasColumnName("LOCATION").HasMaxLength(25);
        builder.Property(c => c.LastChange).HasColumnName("LAST_CHANGE");
        builder.Property(c => c.IsDeleted).HasColumnName("IS_DELETED").HasMaxLength(1);
    }
}
