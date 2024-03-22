using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.Common;

public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property<bool?>("IsActive")
            .HasColumnName("IS_ACTIVE")
            .IsRequired(false)
            .HasDefaultValue(true);

        builder.Property<DateTime>("CreatedAt")
            .HasColumnName("CREATED_AT");

        builder.Property<DateTime>("UpdatedAt")
            .HasColumnName("UPDATED_AT");

        builder.Property<string>("CreationUser")
            .HasMaxLength(256)
            .IsUnicode(false)
            .HasColumnName("CREATION_USER");

        builder.Property<string>("UpdateUser")
            .HasMaxLength(256)
            .IsUnicode(false)
            .HasColumnName("UPDATE_USER");
    }
}