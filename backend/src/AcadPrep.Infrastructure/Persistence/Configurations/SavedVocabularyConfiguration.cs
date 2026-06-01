using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SavedVocabularyConfiguration : IEntityTypeConfiguration<SavedVocabulary>
{
    public void Configure(EntityTypeBuilder<SavedVocabulary> builder)
    {
        builder.ToTable("SAVED_VOCABULARIES");

        builder.HasKey(x => new { x.UserId, x.VocabularyId });

        builder.Property(x => x.Interval)
            .HasDefaultValue(1);

        builder.Property(x => x.DateSaved)
            .HasDefaultValueSql("GETDATE()");

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(u => u.SavedVocabularies)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Vocabulary)
            .WithMany(v => v.SavedVocabularies)
            .HasForeignKey(x => x.VocabularyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
