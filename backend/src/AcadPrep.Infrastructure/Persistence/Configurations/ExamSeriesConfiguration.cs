using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ExamSeriesConfiguration : IEntityTypeConfiguration<ExamSeries>
{
    public void Configure(EntityTypeBuilder<ExamSeries> builder)
    {
        builder.ToTable("EXAM_SERIES");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ExamSeriesId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETDATE()");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
