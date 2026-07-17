using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("NOTIFICATIONS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("NotificationId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.LinkUrl)
            .HasMaxLength(500)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.IsRead)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETDATE()");

        builder.HasIndex(x => new { x.UserId, x.IsRead });

        builder.HasOne(x => x.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
