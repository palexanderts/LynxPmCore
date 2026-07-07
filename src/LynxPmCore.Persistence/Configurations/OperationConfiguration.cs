using LynxPmCore.Domain.Aggregates.Notices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        // Operation apunta directo a la tabla legacy usada por los formularios web/APEX.
        builder.ToTable("LYNX_PM_AVISO_OPERATIONS");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("ID").ValueGeneratedOnAdd();

        // AVISOID es VARCHAR2(20) y no tiene FK real en la tabla legacy (constraint
        // inexistente, valores históricos inconsistentes en filas viejas) — se mantiene
        // así para no arriesgar los formularios web; el dominio sigue exponiendo un int.
        builder.Property(o => o.NoticeId).HasColumnName("AVISOID")
            .HasConversion(v => v.ToString(), v => int.Parse(v))
            .HasMaxLength(20).IsRequired();

        builder.Property(o => o.Code).HasColumnName("OPERATIONCODE").HasMaxLength(20).IsRequired();
        builder.Property(o => o.Description).HasColumnName("DESCRIPTION").HasMaxLength(300).IsRequired();
        builder.Ignore(o => o.Type); // sin mapeo limpio a ACTTYPE, no se persiste (ver plan)
        builder.Property(o => o.Status).HasColumnName("STATUS").HasConversion<int>();
        builder.Property(o => o.Position).HasColumnName("POSITION");
        builder.Property(o => o.StartedAt).HasColumnName("STARTEXECUTIONDATE");
        builder.Property(o => o.CompletedAt).HasColumnName("ENDEXECUTIONDATE");
        builder.Property(o => o.Notes).HasColumnName("OPERACIONTEXT").HasMaxLength(255);
        builder.Property(o => o.ScannedEquipmentCode).HasColumnName("SCANNED_EQUIPMENT_CODE").HasMaxLength(50); // columna nueva, aditiva
        builder.Property(o => o.PhotoConfirmed).HasColumnName("PHOTO_CONFIRMED"); // columna nueva, aditiva
        builder.Property(o => o.AssignedTechnician).HasColumnName("EXECUTORCODE").HasMaxLength(20);
        builder.Property(o => o.Failure).HasColumnName("FAILURE").HasMaxLength(1000); // columna nueva, aditiva
        builder.Property(o => o.CreatedAt).HasColumnName("CREATEDATE");
        builder.Property(o => o.UpdatedAt).HasColumnName("UPDATED_AT"); // columna nueva, aditiva
        builder.Property(o => o.IsDeleted).HasColumnName("IS_DELETED"); // columna nueva, aditiva

        builder.HasIndex(o => o.NoticeId);
    }
}
