using LynxPmCore.Domain.Aggregates.Equipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class EquipmentMediaConfiguration : IEntityTypeConfiguration<EquipmentMedia>
{
    public void Configure(EntityTypeBuilder<EquipmentMedia> builder)
    {
        builder.ToTable("LYNX_PM_EQUIPMENT_MEDIA");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("ID").HasColumnType("RAW(16)");
        builder.Property(m => m.EquipmentCode).HasColumnName("EQUIPMENT_CODE").HasMaxLength(50).IsRequired();
        builder.Property(m => m.MediaType).HasColumnName("MEDIA_TYPE").HasMaxLength(10).IsRequired();
        builder.Property(m => m.Url).HasColumnName("URL").HasMaxLength(2000).IsRequired();
        builder.Property(m => m.ThumbnailUrl).HasColumnName("THUMBNAIL_URL").HasMaxLength(2000);
        builder.Property(m => m.Title).HasColumnName("TITLE").HasMaxLength(200);
        builder.Property(m => m.Position).HasColumnName("POSITION");
        builder.Property(m => m.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(100);
        builder.Property(m => m.CreatedAt).HasColumnName("CREATED_AT");
        builder.Property(m => m.UpdatedAt).HasColumnName("UPDATED_AT");
        builder.Property(m => m.IsDeleted).HasColumnName("IS_DELETED");

        builder.HasIndex(m => m.EquipmentCode);
    }
}
