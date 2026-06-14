using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class StudyStreakConfiguration : IEntityTypeConfiguration<StudyStreak>
{
    public void Configure(EntityTypeBuilder<StudyStreak> builder)
    {
        builder.ToTable("STUDY_STREAKS");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId)
            .ValueGeneratedNever();

        builder.Property(x => x.CurrentStreak)
            .HasDefaultValue(0);

        builder.Property(x => x.MaxStreak)
            .HasDefaultValue(0);

        builder.Property(x => x.LastActiveDate)
            .HasColumnType("date")
            .IsRequired();

        // Relationships (1-to-1)
        builder.HasOne(x => x.User)
            .WithOne(u => u.StudyStreak)
            .HasForeignKey<StudyStreak>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
