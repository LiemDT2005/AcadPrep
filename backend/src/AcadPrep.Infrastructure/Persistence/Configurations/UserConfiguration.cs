using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("USERS", t =>
        {
            t.HasCheckConstraint("CHK_UserStatus", "[Status] IN ('Active', 'Inactive')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("UserId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Email)
            .HasMaxLength(150)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(255)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasDefaultValue(UserStatus.Active)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.RoleId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETDATE()");

        builder.Property(x => x.LastModifiedAt)
            .IsRequired(false);

        // Relationships
        builder.HasOne(x => x.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
