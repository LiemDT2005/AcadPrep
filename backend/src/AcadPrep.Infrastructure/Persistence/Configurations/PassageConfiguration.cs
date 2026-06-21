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
            .IsRequired();

        builder.Property(x => x.ExamId)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Exam)
            .WithMany(e => e.Passages)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
