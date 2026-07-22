using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class QuestionGroupConfiguration : IEntityTypeConfiguration<QuestionGroup>
{
    public void Configure(EntityTypeBuilder<QuestionGroup> builder)
    {
        builder.ToTable("QUESTION_GROUPS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("QuestionGroupId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.AudioUrl)
            .HasMaxLength(500)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.AudioStartSecond)
            .IsRequired(false);

        builder.Property(x => x.AudioEndSecond)
            .IsRequired(false);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.Explanation)
            .IsRequired(false);

        builder.Property(x => x.ExamId)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Exam)
            .WithMany(e => e.QuestionGroups)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
