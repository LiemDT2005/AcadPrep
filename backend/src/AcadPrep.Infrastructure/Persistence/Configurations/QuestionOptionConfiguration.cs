using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("QUESTION_OPTIONS", t =>
        {
            t.HasCheckConstraint("CHK_OptionLetter", "[OptionLetter] IN ('A', 'B', 'C', 'D')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("OptionId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.QuestionId)
            .IsRequired();

        builder.Property(x => x.OptionLetter)
            .HasMaxLength(1)
            .IsUnicode(false)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.OptionText)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Question)
            .WithMany(q => q.QuestionOptions)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
