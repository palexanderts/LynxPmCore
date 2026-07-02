using LynxPmCore.Domain.Aggregates.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class ComponentNotificationConfiguration : IEntityTypeConfiguration<ComponentNotification>
{
    public void Configure(EntityTypeBuilder<ComponentNotification> builder)
    {
        builder.ToTable("LYNX_PM_COMPONENT_NOTIFICATIONS");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("ID").ValueGeneratedNever();
        builder.Property(c => c.AvisoId).HasColumnName("AVISO_ID").HasMaxLength(10);
        builder.Property(c => c.OperationPosition).HasColumnName("OPERATION_POSITION");
        builder.Property(c => c.ProductCode).HasColumnName("PRODUCT_CODE").HasMaxLength(20);
        builder.Property(c => c.Quantity).HasColumnName("QUANTITY");
        builder.Property(c => c.UserCode).HasColumnName("USER_CODE").HasMaxLength(10);
        builder.Property(c => c.Fecha).HasColumnName("FECHA");
        builder.Property(c => c.Comments).HasColumnName("COMMENTS").HasMaxLength(200);
    }
}
