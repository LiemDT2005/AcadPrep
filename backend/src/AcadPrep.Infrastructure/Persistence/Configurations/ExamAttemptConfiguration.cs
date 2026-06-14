using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ExamAttemptConfiguration : IEntityTypeConfiguration<ExamAttempt>
{
    public void Configure(EntityTypeBuilder<ExamAttempt> builder)
    {
        builder.ToTable("EXAM_ATTEMPTS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("AttemptId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ExamId)
            .IsRequired();

        builder.Property(x => x.ListeningScore)
            .HasDefaultValue(0);

        builder.Property(x => x.ReadingScore)
            .HasDefaultValue(0);

        builder.Property(x => x.TotalScore)
            .HasDefaultValue(0);

        builder.Property(x => x.RemainingTime)
            .IsRequired();

        builder.Property(x => x.IsSubmitted)
            .HasDefaultValue(false);

        builder.Property(x => x.StartedAt)
            .HasDefaultValueSql("GETDATE()");

        builder.Property(x => x.CompletedAt)
            .IsRequired(false);

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(u => u.ExamAttempts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Exam)
            .WithMany(e => e.ExamAttempts)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
