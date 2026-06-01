using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) 
    : DbContext(options), IAppDbContext
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Vocabulary> Vocabularies => Set<Vocabulary>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Passage> Passages => Set<Passage>();
    public DbSet<VocabPassage> VocabPassages => Set<VocabPassage>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();
    public DbSet<SavedVocabulary> SavedVocabularies => Set<SavedVocabulary>();
    public DbSet<StudyStreak> StudyStreaks => Set<StudyStreak>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all Fluent API configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
