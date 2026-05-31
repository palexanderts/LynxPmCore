using LynxPmCore.Domain.Aggregates.ErpSync;
using LynxPmCore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class ErpSyncConfigConfiguration : IEntityTypeConfiguration<ErpSyncConfig>
{
    public void Configure(EntityTypeBuilder<ErpSyncConfig> builder)
    {
        builder.ToTable("LYNX_PM_ERP_SYNC_CONFIG");

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

        builder.Property(e => e.ProcessName)
            .HasColumnName("PROCESS_NAME")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.IsEnabled)
            .HasColumnName("IS_ENABLED")
            .HasConversion<int>();

        builder.Property(e => e.ErpUrl)
            .HasColumnName("ERP_URL")
            .HasMaxLength(500);

        builder.Property(e => e.AuthHeader)
            .HasColumnName("AUTH_HEADER")
            .HasMaxLength(500);

        builder.Property(e => e.RetryMax)
            .HasColumnName("RETRY_MAX")
            .HasDefaultValue(3);

        builder.Property(e => e.RetryDelaySeconds)
            .HasColumnName("RETRY_DELAY_SEC")
            .HasDefaultValue(60);

        builder.Property(e => e.Priority)
            .HasColumnName("PRIORITY")
            .HasDefaultValue(10);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CREATED_AT");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("UPDATED_AT");

        builder.HasIndex(e => new { e.ClientCode, e.Process })
            .IsUnique()
            .HasDatabaseName("UQ_ERP_SYNC_CONFIG_CLIENT_PROC");
    }
}
