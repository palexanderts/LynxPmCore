using LynxPmCore.Domain.Aggregates.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class ComponentReceiptConfiguration : IEntityTypeConfiguration<ComponentReceipt>
{
    public void Configure(EntityTypeBuilder<ComponentReceipt> builder)
    {
        builder.ToTable("LYNX_PM_COMPONENT_RECEIPTS");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("ID").HasColumnType("RAW(16)");
        builder.Property(r => r.ReceiptId).HasColumnName("RECEIPT_ID").HasMaxLength(50).IsRequired();
        builder.Property(r => r.ComponentId).HasColumnName("COMPONENT_ID").HasMaxLength(200).IsRequired();
        builder.Property(r => r.Quantity).HasColumnName("QUANTITY").IsRequired();
        builder.Property(r => r.Observations).HasColumnName("OBSERVATIONS").HasMaxLength(1000);
        builder.Property(r => r.ReceivedBy).HasColumnName("RECEIVED_BY").HasMaxLength(100).IsRequired();
        builder.Property(r => r.ReceivedAt).HasColumnName("RECEIVED_AT").IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("UPDATED_AT").IsRequired();
        builder.Property(r => r.IsDeleted).HasColumnName("IS_DELETED").IsRequired();

        builder.HasIndex(r => r.ReceiptId).IsUnique();
        builder.HasIndex(r => r.ComponentId);
        builder.HasIndex(r => new { r.ComponentId, r.ReceivedAt, r.ReceivedBy });
    }
}
