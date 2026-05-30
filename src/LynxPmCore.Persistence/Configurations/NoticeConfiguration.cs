using LynxPmCore.Domain.Aggregates.Notices;
using LynxPmCore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class NoticeConfiguration : IEntityTypeConfiguration<Notice>
{
    public void Configure(EntityTypeBuilder<Notice> builder)
    {
        builder.ToTable("LYNX_PM_NOTICES");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("ID").HasColumnType("RAW(16)");
        builder.Property(n => n.Number).HasColumnName("NOTICE_NUMBER").HasMaxLength(50).IsRequired();
        builder.Property(n => n.EquipmentCode).HasColumnName("EQUIPMENT_CODE").HasMaxLength(50).IsRequired();
        builder.Property(n => n.Description).HasColumnName("DESCRIPTION").HasMaxLength(500);
        builder.Property(n => n.Status).HasColumnName("STATUS").HasConversion<int>();
        builder.Property(n => n.IsApproved).HasColumnName("IS_APPROVED");
        builder.Property(n => n.ApexId).HasColumnName("APEX_ID").HasMaxLength(100);
        builder.Property(n => n.IsSynchronized).HasColumnName("IS_SYNCHRONIZED");
        builder.Property(n => n.SynchronizedAt).HasColumnName("SYNCHRONIZED_AT");
        builder.Property(n => n.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(100).IsRequired();
        builder.Property(n => n.ApprovedBy).HasColumnName("APPROVED_BY").HasMaxLength(100);
        builder.Property(n => n.Location).HasColumnName("LOCATION").HasMaxLength(200);
        builder.Property(n => n.Customer).HasColumnName("CUSTOMER").HasMaxLength(100);
        builder.Property(n => n.Priority).HasColumnName("PRIORITY");
        builder.Property(n => n.CreatedAt).HasColumnName("CREATED_AT");
        builder.Property(n => n.UpdatedAt).HasColumnName("UPDATED_AT");
        builder.Property(n => n.IsDeleted).HasColumnName("IS_DELETED");

        builder.HasIndex(n => n.Number).IsUnique();
        builder.HasIndex(n => n.EquipmentCode);
        builder.HasIndex(n => n.Status);

        builder.HasMany(n => n.Operations)
            .WithOne()
            .HasForeignKey(o => o.NoticeId);
        builder.Navigation(n => n.Operations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
