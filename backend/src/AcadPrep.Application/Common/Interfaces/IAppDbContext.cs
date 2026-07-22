using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Role> Roles { get; }
    DbSet<Exam> Exams { get; }
    DbSet<Vocabulary> Vocabularies { get; }
    DbSet<User> Users { get; }
    DbSet<Passage> Passages { get; }
    DbSet<VocabPassage> VocabPassages { get; }
    DbSet<Question> Questions { get; }
    DbSet<ExamSeries> ExamSeries { get; }
    DbSet<ExamAttempt> ExamAttempts { get; }
    DbSet<QuestionOption> QuestionOptions { get; }
    DbSet<AttemptAnswer> AttemptAnswers { get; }
    DbSet<SavedVocabulary> SavedVocabularies { get; }
    DbSet<StudyStreak> StudyStreaks { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Notification> Notifications { get; }

    DbSet<Achievement> Achievements { get; }
    DbSet<UserAchievement> UserAchievements { get; }
    DbSet<QuestionGroup> QuestionGroups { get; }
    DbSet<PracticeSession> PracticeSessions { get; }
    DbSet<Plan> Plans { get; }
    DbSet<Order> Orders { get; }
    DbSet<UserSubscription> UserSubscriptions { get; }
    DbSet<PaymentWebhookLog> PaymentWebhookLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
