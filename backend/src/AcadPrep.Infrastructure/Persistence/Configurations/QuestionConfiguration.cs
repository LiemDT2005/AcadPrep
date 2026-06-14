using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("QUESTIONS", t =>
        {
            t.HasCheckConstraint("CHK_QuestionPart", "[Part] BETWEEN 1 AND 7");
            t.HasCheckConstraint("CHK_CorrectOption", "[CorrectOption] IN ('A', 'B', 'C', 'D')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("QuestionId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.QuestionNumber)
            .IsRequired();

        builder.Property(x => x.Part)
            .IsRequired();

        builder.Property(x => x.QuestionText)
            .IsRequired(false);

        builder.Property(x => x.AudioUrl)
            .HasMaxLength(500)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.CorrectOption)
            .HasMaxLength(1)
            .IsUnicode(false)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.ExamId)
            .IsRequired();

        builder.Property(x => x.PassageId)
            .IsRequired(false);

        // Relationships
        builder.HasOne(x => x.Exam)
            .WithMany(e => e.Questions)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Passage)
            .WithMany(p => p.Questions)
            .HasForeignKey(x => x.PassageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
