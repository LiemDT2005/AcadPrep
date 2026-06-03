using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable("EXAMS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ExamId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired(false);

        builder.Property(x => x.Duration)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETDATE()");

        builder.Property(x => x.LastModifiedAt)
            .IsRequired(false);

        // Global query filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
