using LynxPmCore.Domain.Aggregates.Notices;
using LynxPmCore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class NoticeConfiguration : IEntityTypeConfiguration<Notice>
{
    public void Configure(EntityTypeBuilder<Notice> builder)
    {
        // Notice apunta directo a la tabla legacy usada por los formularios web/APEX.
        // Solo se mapea el subconjunto de ~80 columnas que el dominio ya modela hoy;
        // el resto (SAP, rental, geolocalización, etc.) queda intacto sin tocar.
        builder.ToTable("LYNX_PM_AVISO");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("ID").ValueGeneratedOnAdd();

        // AVISOSAP es el candidato más poblado/legible como "número de negocio" en los
        // datos reales, pero es nullable — algunas filas legacy antiguas solo tienen
        // SAPAVISOID o ninguno de los dos. Se tolera null -> "" en lectura (limitación
        // conocida, ya documentada en el corte anterior; no se intenta el fallback
        // entre columnas a nivel de EF).
        builder.Property(n => n.Number).HasColumnName("AVISOSAP")
            .HasConversion(v => v, v => v ?? string.Empty)
            .HasMaxLength(20).IsRequired(false);

        builder.Property(n => n.EquipmentCode).HasColumnName("EQUIPMENT").HasMaxLength(20).IsRequired();

        // FAILLONGDESCRIPTION (300 chars) es donde en la práctica queda la descripción
        // general del aviso (visto en datos reales) — la columna DESCRIPTION (50 chars)
        // es demasiado chica para el texto libre de "Síntoma" que envía la app.
        builder.Property(n => n.Description).HasColumnName("FAILLONGDESCRIPTION").HasMaxLength(300);

        builder.Property(n => n.Status).HasColumnName("STATUS").HasConversion<int>();
        builder.Property(n => n.IsApproved).HasColumnName("IS_APPROVED"); // columna nueva, aditiva
        builder.Property(n => n.ApprovedBy).HasColumnName("CHECKBY").HasMaxLength(15);
        builder.Property(n => n.RejectionReason).HasColumnName("MOTIVO_COMMENTS").HasMaxLength(500);
        builder.Ignore(n => n.ApprovalStatus);
        builder.Property(n => n.IsSynchronized).HasColumnName("SYNC").HasConversion<int>();
        builder.Property(n => n.SynchronizedAt).HasColumnName("SYNCHRONIZED_AT"); // columna nueva, aditiva
        builder.Property(n => n.CreatedBy).HasColumnName("USUARIO").HasMaxLength(15).IsRequired();
        builder.Property(n => n.Location).HasColumnName("LOCATION").HasMaxLength(20);
        builder.Property(n => n.Customer).HasColumnName("CUSTOMER").HasMaxLength(50);
        builder.Property(n => n.Center).HasColumnName("CENTERPLAN").HasMaxLength(20);
        builder.Ignore(n => n.Priority); // sin columna NUMBER equivalente en legacy
        builder.Property(n => n.PriorityCode).HasColumnName("PRIORITY").HasMaxLength(20);
        builder.Ignore(n => n.PriorityText); // se resuelve por catálogo, no se persiste
        builder.Property(n => n.NoticeTypeCode).HasColumnName("TYPE").HasMaxLength(15);
        builder.Ignore(n => n.NoticeTypeText); // se resuelve por catálogo (LYNX_PM_AVISO_TYPE), no se persiste
        builder.Property(n => n.CreatedAt).HasColumnName("CREATEDDATE");
        builder.Property(n => n.UpdatedAt).HasColumnName("UPDATEDATE");
        builder.Property(n => n.IsDeleted).HasColumnName("IS_DELETED"); // columna nueva, aditiva (Notice)

        builder.HasIndex(n => n.Number).IsUnique();
        builder.HasIndex(n => n.EquipmentCode);
        builder.HasIndex(n => n.Status);

        builder.HasMany(n => n.Operations)
            .WithOne()
            .HasForeignKey(o => o.NoticeId);
        builder.Navigation(n => n.Operations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(n => n.Causes)
            .WithOne()
            .HasForeignKey(c => c.NoticeId);
        builder.Navigation(n => n.Causes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
