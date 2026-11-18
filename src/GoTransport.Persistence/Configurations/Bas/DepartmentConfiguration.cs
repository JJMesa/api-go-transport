using GoTransport.Domain.Entities.Bas;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.Bas;

public class DepartmentConfiguration : BaseEntityConfiguration<Department>
{
    public override void Configure(EntityTypeBuilder<Department> builder)
    {
        base.Configure(builder);

        builder.ToTable("BAS_DEPARTMENT");

        builder.HasKey(e => e.DepartmentId);

        builder.Property(e => e.DepartmentId)
            .HasColumnType("int")
            .HasColumnName("DEPARTMENT_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.Description)
            .HasMaxLength(32)
            .HasColumnType("varchar(32)")
            .IsUnicode(false)
            .HasColumnName("DESCRIPTION")
            .HasColumnOrder(2);
    }
}