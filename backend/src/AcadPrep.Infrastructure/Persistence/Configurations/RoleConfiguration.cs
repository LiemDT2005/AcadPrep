using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("ROLES");

        builder.HasKey(x => x.RoleId);

        builder.Property(x => x.RoleId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.RoleName)
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(x => x.RoleName)
            .IsUnique();
    }
}
