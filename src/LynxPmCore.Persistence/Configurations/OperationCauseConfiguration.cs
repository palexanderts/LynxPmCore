using LynxPmCore.Domain.Aggregates.Notices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class OperationCauseConfiguration : IEntityTypeConfiguration<OperationCause>
{
    public void Configure(EntityTypeBuilder<OperationCause> builder)
    {
        builder.ToTable("LYNXCORE_OPERATION_CAUSES");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("ID").HasColumnType("RAW(16)").ValueGeneratedNever();
        builder.Property(c => c.OperationId).HasColumnName("OPERATION_ID").HasColumnType("RAW(16)").IsRequired();
        builder.Property(c => c.Code).HasColumnName("CODE").HasMaxLength(20).IsRequired();
        builder.Property(c => c.Text).HasColumnName("TEXT").HasMaxLength(500);
        builder.Property(c => c.CreatedAt).HasColumnName("CREATED_AT");
        builder.Property(c => c.UpdatedAt).HasColumnName("UPDATED_AT");
        builder.Property(c => c.IsDeleted).HasColumnName("IS_DELETED");

        builder.HasIndex(c => c.OperationId);
    }
}
