using LynxPmCore.Domain.Aggregates.ErpSync;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class ErpSyncOutboxConfiguration : IEntityTypeConfiguration<ErpSyncOutboxEntry>
{
    public void Configure(EntityTypeBuilder<ErpSyncOutboxEntry> builder)
    {
        builder.ToTable("LYNX_PM_ERP_SYNC_OUTBOX");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("ID")
            .HasColumnType("RAW(16)")
            .HasConversion(
                id => id.ToByteArray(),
                bytes => new Guid(bytes));

        builder.Property(e => e.ClientCode)
            .HasColumnName("CLIENT_CODE")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Process)
            .HasColumnName("PROCESS_CODE")
            .HasConversion<int>();

        builder.Property(e => e.EntityId)
            .HasColumnName("ENTITY_ID")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Payload)
            .HasColumnName("PAYLOAD")
            .HasColumnType("CLOB")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("STATUS")
            .HasConversion<int>();

        builder.Property(e => e.AttemptCount)
            .HasColumnName("ATTEMPT_COUNT")
            .HasDefaultValue(0);

        builder.Property(e => e.LastError)
            .HasColumnName("LAST_ERROR")
            .HasColumnType("CLOB");

        builder.Property(e => e.ScheduledAt)
            .HasColumnName("SCHEDULED_AT");

        builder.Property(e => e.ProcessedAt)
            .HasColumnName("PROCESSED_AT");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CREATED_AT");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("UPDATED_AT");

        builder.Ignore(e => e.IsDeleted);

        builder.HasIndex(e => new { e.Status, e.ScheduledAt })
            .HasDatabaseName("IX_ERP_OUTBOX_STATUS_SCHEDULED");

        builder.HasIndex(e => e.EntityId)
            .HasDatabaseName("IX_ERP_OUTBOX_ENTITY");
    }
}
