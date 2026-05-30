using LynxPmCore.Domain.Aggregates.Notices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.ToTable("LYNXCORE_OPERATIONS");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("ID").HasColumnType("RAW(16)");
        builder.Property(o => o.NoticeId).HasColumnName("NOTICE_ID").HasColumnType("RAW(16)").IsRequired();
        builder.Property(o => o.Code).HasColumnName("CODE").HasMaxLength(50).IsRequired();
        builder.Property(o => o.Description).HasColumnName("DESCRIPTION").HasMaxLength(500).IsRequired();
        builder.Property(o => o.Type).HasColumnName("TYPE").HasConversion<int>();
        builder.Property(o => o.Status).HasColumnName("STATUS").HasConversion<int>();
        builder.Property(o => o.Position).HasColumnName("POSITION");
        builder.Property(o => o.StartedAt).HasColumnName("STARTED_AT");
        builder.Property(o => o.CompletedAt).HasColumnName("COMPLETED_AT");
        builder.Property(o => o.Notes).HasColumnName("NOTES").HasMaxLength(1000);
        builder.Property(o => o.ScannedEquipmentCode).HasColumnName("SCANNED_EQUIPMENT_CODE").HasMaxLength(50);
        builder.Property(o => o.PhotoConfirmed).HasColumnName("PHOTO_CONFIRMED");
        builder.Property(o => o.AssignedTechnician).HasColumnName("ASSIGNED_TECHNICIAN").HasMaxLength(100);
        builder.Property(o => o.CreatedAt).HasColumnName("CREATED_AT");
        builder.Property(o => o.UpdatedAt).HasColumnName("UPDATED_AT");
        builder.Property(o => o.IsDeleted).HasColumnName("IS_DELETED");

        builder.HasIndex(o => o.NoticeId);
    }
}
