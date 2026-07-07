using LynxPmCore.Domain.Aggregates.Notices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class NoticeCauseConfiguration : IEntityTypeConfiguration<NoticeCause>
{
    public void Configure(EntityTypeBuilder<NoticeCause> builder)
    {
        // Tabla nueva y separada — no toca LYNX_PM_AVISO ni las columnas CAUSE/FAIL
        // nativas (ligadas a la sincronización SAP). FK real hacia LYNX_PM_AVISO(ID).
        builder.ToTable("LYNX_PM_AVISO_CAUSE");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("ID").ValueGeneratedOnAdd();
        builder.Property(c => c.NoticeId).HasColumnName("AVISO_ID").IsRequired();
        builder.Property(c => c.Code).HasColumnName("CODE").HasMaxLength(20).IsRequired();
        builder.Property(c => c.Text).HasColumnName("TEXT").HasMaxLength(500);
        builder.Property(c => c.CreatedAt).HasColumnName("CREATED_AT");
        builder.Property(c => c.IsDeleted).HasColumnName("IS_DELETED");
        builder.Ignore(c => c.UpdatedAt); // la tabla nueva no tiene columna UPDATED_AT

        builder.HasIndex(c => c.NoticeId);
    }
}
