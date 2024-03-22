using GoTransport.Domain.Entities.Bas;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.Bas;

internal class CityConfiguration : BaseEntityConfiguration<City>
{
    public override void Configure(EntityTypeBuilder<City> builder)
    {
        base.Configure(builder);

        builder.ToTable("BAS_CITY");

        builder.HasKey(e => e.CityId);

        builder.Property(e => e.CityId)
            .HasColumnType("int")
            .HasColumnName("CITY_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.Description)
            .HasMaxLength(32)
            .HasColumnType("varchar(32)")
            .IsUnicode(false)
            .HasColumnName("DESCRIPTION")
            .HasColumnOrder(2);

        builder.Property(e => e.DepartmentId)
            .HasColumnType("int")
            .HasColumnName("DEPARTMENT_ID")
            .HasColumnOrder(3);
    }
}