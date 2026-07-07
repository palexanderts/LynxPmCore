using LynxPmCore.Domain.Aggregates.Notices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class OperationPartConfiguration : IEntityTypeConfiguration<OperationPart>
{
    public void Configure(EntityTypeBuilder<OperationPart> builder)
    {
        // Partes objeto reportadas al notificar una operacion. FK real hacia
        // LYNX_PM_AVISO_OPERATIONS(ID) -- Operation.Id es un NUMBER limpio,
        // a diferencia de AVISOID (VARCHAR2 sin FK real por datos historicos).
        builder.ToTable("LYNX_PM_OPERATION_PART");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("ID").ValueGeneratedOnAdd();
        builder.Property(p => p.OperationId).HasColumnName("OPERATION_ID").IsRequired();
        builder.Property(p => p.Code).HasColumnName("CODE").HasMaxLength(20).IsRequired();
        builder.Property(p => p.Text).HasColumnName("TEXT").HasMaxLength(500);
        builder.Property(p => p.CreatedAt).HasColumnName("CREATED_AT");
        builder.Property(p => p.IsDeleted).HasColumnName("IS_DELETED");
        builder.Ignore(p => p.UpdatedAt); // la tabla no tiene columna UPDATED_AT

        builder.HasIndex(p => p.OperationId);
    }
}
