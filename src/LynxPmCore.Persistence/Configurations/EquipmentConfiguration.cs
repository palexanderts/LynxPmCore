using LynxPmCore.Domain.Aggregates.Equipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("LYNX_PM_EQUIPMENTS");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("ID").HasColumnType("RAW(16)");
        builder.Property(e => e.Code).HasColumnName("CODE").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Description).HasColumnName("DESCRIPTION").HasMaxLength(500).IsRequired();
        builder.Property(e => e.Location).HasColumnName("LOCATION").HasMaxLength(200);
        builder.Property(e => e.Customer).HasColumnName("CUSTOMER").HasMaxLength(100);
        builder.Property(e => e.ParentCode).HasColumnName("PARENT_CODE").HasMaxLength(50);
        builder.Property(e => e.IsActive).HasColumnName("IS_ACTIVE");
        builder.Property(e => e.LastSyncAt).HasColumnName("LAST_SYNC_AT");
        builder.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
        builder.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
        builder.Property(e => e.IsDeleted).HasColumnName("IS_DELETED");

        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.Customer);
    }
}
