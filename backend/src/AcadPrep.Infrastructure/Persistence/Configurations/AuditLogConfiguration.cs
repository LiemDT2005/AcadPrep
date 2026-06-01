using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AUDITLOGS");

        builder.HasKey(x => x.LogId);

        builder.Property(x => x.LogId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .IsRequired(false);

        builder.Property(x => x.Action)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.TableAffected)
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.Timestamp)
            .HasDefaultValueSql("GETDATE()");

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
