using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AttemptAnswerConfiguration : IEntityTypeConfiguration<AttemptAnswer>
{
    public void Configure(EntityTypeBuilder<AttemptAnswer> builder)
    {
        builder.ToTable("ATTEMPT_ANSWERS", t =>
        {
            t.HasCheckConstraint("CHK_SelectedOption", "[SelectedOption] IN ('A', 'B', 'C', 'D')");
        });

        builder.HasKey(x => new { x.AttemptId, x.QuestionId });

        builder.Property(x => x.SelectedOption)
            .HasMaxLength(1)
            .IsUnicode(false)
            .HasConversion<string>()
            .IsRequired(false);

        builder.Property(x => x.IsCorrect)
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(x => x.ExamAttempt)
            .WithMany(a => a.AttemptAnswers)
            .HasForeignKey(x => x.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Question)
            .WithMany(q => q.AttemptAnswers)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
