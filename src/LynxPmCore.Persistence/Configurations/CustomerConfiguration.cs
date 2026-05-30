using LynxPmCore.Domain.Aggregates.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LynxPmCore.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("LYNX_PM_CUSTOMERS");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("ID").HasColumnType("RAW(16)");
        builder.Property(c => c.Code).HasColumnName("CODE").HasMaxLength(50).IsRequired();
        builder.Property(c => c.Name).HasColumnName("NAME").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Address).HasColumnName("ADDRESS").HasMaxLength(500);
        builder.Property(c => c.Phone).HasColumnName("PHONE").HasMaxLength(50);
        builder.Property(c => c.IsActive).HasColumnName("IS_ACTIVE");
        builder.Property(c => c.CreatedAt).HasColumnName("CREATED_AT");
        builder.Property(c => c.UpdatedAt).HasColumnName("UPDATED_AT");
        builder.Property(c => c.IsDeleted).HasColumnName("IS_DELETED");

        builder.HasIndex(c => c.Code).IsUnique();
    }
}
