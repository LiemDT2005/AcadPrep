using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VocabPassageConfiguration : IEntityTypeConfiguration<VocabPassage>
{
    public void Configure(EntityTypeBuilder<VocabPassage> builder)
    {
        builder.ToTable("VOCAB_PASSAGES");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("VocabPassageId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.VocabularyId)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Vocabulary)
            .WithMany(v => v.VocabPassages)
            .HasForeignKey(x => x.VocabularyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
