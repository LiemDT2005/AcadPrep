using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PassageConfiguration : IEntityTypeConfiguration<Passage>
{
    public void Configure(EntityTypeBuilder<Passage> builder)
    {
        builder.ToTable("PASSAGES");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("PassageId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Content)
            .IsRequired(false);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.ExamId)
            .IsRequired();

        builder.Property(x => x.QuestionGroupId)
            .IsRequired(false);

        // Relationships
        builder.HasOne(x => x.Exam)
            .WithMany(e => e.Passages)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.QuestionGroup)
            .WithMany(g => g.Passages)
            .HasForeignKey(x => x.QuestionGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.QuestionGroupId, x.DisplayOrder });
    }
}
