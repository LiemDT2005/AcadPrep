using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) 
    : DbContext(options), IAppDbContext
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Vocabulary> Vocabularies => Set<Vocabulary>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Passage> Passages => Set<Passage>();
    public DbSet<VocabPassage> VocabPassages => Set<VocabPassage>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
    public DbSet<ExamSeries> ExamSeries => Set<ExamSeries>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();
    public DbSet<SavedVocabulary> SavedVocabularies => Set<SavedVocabulary>();
    public DbSet<StudyStreak> StudyStreaks => Set<StudyStreak>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<QuestionGroup> QuestionGroups => Set<QuestionGroup>();
    public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all Fluent API configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
