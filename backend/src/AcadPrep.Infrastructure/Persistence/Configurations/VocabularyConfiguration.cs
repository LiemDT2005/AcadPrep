using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VocabularyConfiguration : IEntityTypeConfiguration<Vocabulary>
{
    public void Configure(EntityTypeBuilder<Vocabulary> builder)
    {
        builder.ToTable("VOCABULARIES");

        builder.HasKey(x => x.VocabularyId);

        builder.Property(x => x.VocabularyId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Word)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Word)
            .IsUnique();

        builder.Property(x => x.Phonetic)
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.Meaning)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Example)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
    }
}
