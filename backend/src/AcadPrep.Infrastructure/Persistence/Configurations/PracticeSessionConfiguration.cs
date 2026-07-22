using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PracticeSessionConfiguration : IEntityTypeConfiguration<PracticeSession>
{
    public void Configure(EntityTypeBuilder<PracticeSession> builder)
    {
        builder.ToTable("PRACTICE_SESSIONS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("PracticeSessionId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ExamId)
            .IsRequired();

        builder.Property(x => x.SelectedParts)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SelectedTags)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.TimeLimit)
            .IsRequired(false);

        builder.Property(x => x.CombinedQuestionsList)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.AnswersJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(x => x.IsSubmitted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CompletedAt)
            .IsRequired(false);

        builder.Property(x => x.CorrectCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.TotalQuestions)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.ListeningCorrect)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.ReadingCorrect)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.ListeningTotal)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.ReadingTotal)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Exam)
            .WithMany()
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
