using LynxPmCore.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("LYNX_PM_OUTBOX");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("ID").HasColumnType("RAW(16)");
        builder.Property(o => o.Type).HasColumnName("TYPE").HasMaxLength(500).IsRequired();
        builder.Property(o => o.Content).HasColumnName("CONTENT").HasColumnType("CLOB").IsRequired();
        builder.Property(o => o.OccurredOnUtc).HasColumnName("OCCURRED_ON_UTC");
        builder.Property(o => o.ProcessedOnUtc).HasColumnName("PROCESSED_ON_UTC");
        builder.Property(o => o.Error).HasColumnName("ERROR").HasMaxLength(2000);
        builder.Property(o => o.RetryCount).HasColumnName("RETRY_COUNT");

        builder.HasIndex(o => o.ProcessedOnUtc);
    }
}
