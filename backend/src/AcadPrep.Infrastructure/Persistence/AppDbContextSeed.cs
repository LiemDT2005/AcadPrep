using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public static class AppDbContextSeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        // ── 1. Roles ──
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { RoleName = "Admin" },
                new Role { RoleName = "User" }
            );
            await context.SaveChangesAsync();
        }

        // ── 2. Users ──
        if (!context.Users.Any())
        {
            var adminRole = context.Roles.First(r => r.RoleName == "Admin");
            var userRole  = context.Roles.First(r => r.RoleName == "User");

            context.Users.AddRange(
                new User
                {
                    Email = "admin@acadprep.com",
                    PasswordHash = "hashed_password",
                    FullName = "Admin User",
                    RoleId = adminRole.RoleId,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new User
                {
                    Email = "user@acadprep.com",
                    PasswordHash = "hashed_password",
                    FullName = "Test User",
                    RoleId = userRole.RoleId,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new User
                {
                    Email = "hana@acadprep.com",
                    PasswordHash = "hashed_password",
                    FullName = "Hana Nguyen",
                    RoleId = userRole.RoleId,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new User
                {
                    Email = "minh@acadprep.com",
                    PasswordHash = "hashed_password",
                    FullName = "Minh Tran",
                    RoleId = userRole.RoleId,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new User
                {
                    Email = "inactive@acadprep.com",
                    PasswordHash = "hashed_password",
                    FullName = "Inactive User",
                    RoleId = userRole.RoleId,
                    Status = UserStatus.Inactive,
                    CreatedAt = DateTime.UtcNow.AddDays(-60)
                }
            );
            await context.SaveChangesAsync();
        }

        // ── 3. Exams ──
        if (!context.Exams.Any())
        {
            context.Exams.AddRange(
                new Exam { Title = "ETS TOEIC 2025 Test 1", Duration = 120, Description = "Official Practice Test",          CreatedAt = DateTime.UtcNow.AddDays(-25) },
                new Exam { Title = "Economy TOEIC Vol 5",   Duration = 120, Description = "Trending Practice Test",          CreatedAt = DateTime.UtcNow.AddDays(-20) },
                new Exam { Title = "Hacker TOEIC Practice", Duration = 120, Description = "Hard difficulty Practice Test",   CreatedAt = DateTime.UtcNow.AddDays(-15) },
                new Exam { Title = "ETS TOEIC 2025 Test 2", Duration = 120, Description = "Official Practice Test 2",       CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new Exam { Title = "Economy TOEIC Vol 6",   Duration = 120, Description = "Latest Economy edition",         CreatedAt = DateTime.UtcNow.AddDays(-5) }
            );
            await context.SaveChangesAsync();
        }

        // ── 3.5. Questions ──
        if (!context.Questions.Any())
        {
            var examsList = context.Exams.ToList();
            foreach(var exam in examsList)
            {
                // Seed 7 questions per exam, one for each Part (1 to 7)
                for (int p = 1; p <= 7; p++)
                {
                    context.Questions.Add(new Question
                    {
                        ExamId = exam.Id,
                        Part = p,
                        QuestionNumber = p,
                        QuestionText = $"Mock Question for Part {p} of {exam.Title}",
                        CorrectOption = OptionLetter.A
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        // ── 4. Achievements ──
        if (!context.Achievements.Any())
        {
            context.Achievements.AddRange(
                new Achievement { Name = "First Blood",     Description = "Complete your first exam",           IconUrl = "https://cdn-icons-png.flaticon.com/512/1000/1000185.png", ConditionType = "ExamsCompleted", ConditionValue = 1 },
                new Achievement { Name = "Perfect Score",   Description = "Get a perfect score on any exam",    IconUrl = "https://cdn-icons-png.flaticon.com/512/1000/1000184.png", ConditionType = "Score", ConditionValue = 990 },
                new Achievement { Name = "Streak Master",   Description = "Maintain a 7-day study streak",      IconUrl = "https://cdn-icons-png.flaticon.com/512/4302/4302196.png", ConditionType = "Streak", ConditionValue = 7 },
                new Achievement { Name = "Word Collector",  Description = "Save 50 vocabularies",               IconUrl = "https://cdn-icons-png.flaticon.com/512/2232/2232688.png", ConditionType = "VocabCount", ConditionValue = 50 },
                new Achievement { Name = "Speed Demon",     Description = "Complete a full test in under 90 minutes", IconUrl = "https://cdn-icons-png.flaticon.com/512/3524/3524388.png", ConditionType = "Speed", ConditionValue = 90 }
            );
            await context.SaveChangesAsync();
        }

        // ── 5. Vocabularies ──
        if (!context.Vocabularies.Any())
        {
            context.Vocabularies.AddRange(
                new Vocabulary { Word = "Accommodate",    Phonetic = "/ə'kɑːmədeɪt/",       Meaning = "To provide lodging or sufficient space",      Example = "The hotel can accommodate up to 500 guests." },
                new Vocabulary { Word = "Comprehensive",  Phonetic = "/ˌkɑːmprɪ'hensɪv/",   Meaning = "Complete; including all or nearly all elements", Example = "We offer a comprehensive training program." },
                new Vocabulary { Word = "Implement",      Phonetic = "/'ɪmplɪment/",         Meaning = "To put into effect",            Example = "The changes to the national health system will be implemented next year." },
                new Vocabulary { Word = "Crucial",        Phonetic = "/'kruːʃl/",             Meaning = "Decisive or critical, especially in the success or failure of something", Example = "It is crucial that we arrive before 8 o'clock." },
                new Vocabulary { Word = "Substantial",    Phonetic = "/səb'stænʃl/",          Meaning = "Of considerable importance, size, or worth",    Example = "She inherited a substantial fortune from her grandmother." },
                new Vocabulary { Word = "Negotiate",      Phonetic = "/nɪ'goʊʃieɪt/",        Meaning = "Try to reach an agreement or compromise",         Example = "We managed to negotiate a good deal." },
                new Vocabulary { Word = "Perspective",    Phonetic = "/pər'spektɪv/",         Meaning = "A particular attitude toward or way of regarding something", Example = "Try to see the issue from a different perspective." },
                new Vocabulary { Word = "Collaborate",    Phonetic = "/kə'læbəreɪt/",        Meaning = "Work jointly on an activity or project",             Example = "We need to collaborate with other teams on this project." },
                new Vocabulary { Word = "Resilient",      Phonetic = "/rɪ'zɪliənt/",          Meaning = "Able to withstand or recover quickly from difficult conditions", Example = "She is a resilient person who always bounces back from difficulties." },
                new Vocabulary { Word = "Preliminary",    Phonetic = "/prɪ'lɪmɪneri/",       Meaning = "Denoting an action or event preceding or done in preparation", Example = "The preliminary results look promising." }
            );
            await context.SaveChangesAsync();
        }

        // ── 6. SavedVocabularies (for Notebook + Review) ──
        if (!context.SavedVocabularies.Any())
        {
            var testUser = context.Users.FirstOrDefault(u => u.Email == "user@acadprep.com");
            var allVocabs = context.Vocabularies.ToList();

            if (testUser != null && allVocabs.Any())
            {
                // Save all 10 vocabs for test user — some due for review, some not
                for (int i = 0; i < allVocabs.Count; i++)
                {
                    context.SavedVocabularies.Add(new SavedVocabulary
                    {
                        UserId = testUser.Id,
                        VocabularyId = allVocabs[i].Id,
                        Interval = i < 3 ? 1 : (i < 6 ? 2 : 4),
                        DateSaved = DateTime.UtcNow.AddDays(-(10 - i)),
                        // First 5 words are due today or overdue → will show in Review
                        NextReviewDate = i < 5
                            ? DateTime.UtcNow.AddDays(-1).Date
                            : DateTime.UtcNow.AddDays(i).Date
                    });
                }
                await context.SaveChangesAsync();
            }
        }

        // ── 7. ExamAttempts (multiple for ScoreProgress chart) ──
        if (!context.ExamAttempts.Any())
        {
            var testUser = context.Users.FirstOrDefault(u => u.Email == "user@acadprep.com");
            var hana     = context.Users.FirstOrDefault(u => u.Email == "hana@acadprep.com");
            var exams    = context.Exams.ToList();

            if (testUser != null && exams.Count >= 5)
            {
                // Test User: 5 attempts showing progress over time
                context.ExamAttempts.AddRange(
                    new ExamAttempt
                    {
                        UserId = testUser.Id, ExamId = exams[0].Id,
                        StartedAt = DateTime.UtcNow.AddDays(-28), CompletedAt = DateTime.UtcNow.AddDays(-28).AddMinutes(115),
                        ListeningScore = 280, ReadingScore = 250, TotalScore = 530, IsSubmitted = true
                    },
                    new ExamAttempt
                    {
                        UserId = testUser.Id, ExamId = exams[1].Id,
                        StartedAt = DateTime.UtcNow.AddDays(-21), CompletedAt = DateTime.UtcNow.AddDays(-21).AddMinutes(110),
                        ListeningScore = 320, ReadingScore = 290, TotalScore = 610, IsSubmitted = true
                    },
                    new ExamAttempt
                    {
                        UserId = testUser.Id, ExamId = exams[2].Id,
                        StartedAt = DateTime.UtcNow.AddDays(-14), CompletedAt = DateTime.UtcNow.AddDays(-14).AddMinutes(105),
                        ListeningScore = 350, ReadingScore = 310, TotalScore = 660, IsSubmitted = true
                    },
                    new ExamAttempt
                    {
                        UserId = testUser.Id, ExamId = exams[3].Id,
                        StartedAt = DateTime.UtcNow.AddDays(-7), CompletedAt = DateTime.UtcNow.AddDays(-7).AddMinutes(100),
                        ListeningScore = 380, ReadingScore = 340, TotalScore = 720, IsSubmitted = true
                    },
                    new ExamAttempt
                    {
                        UserId = testUser.Id, ExamId = exams[4].Id,
                        StartedAt = DateTime.UtcNow.AddDays(-2), CompletedAt = DateTime.UtcNow.AddDays(-2).AddMinutes(95),
                        ListeningScore = 415, ReadingScore = 370, TotalScore = 785, IsSubmitted = true
                    }
                );

                // Hana: 3 attempts
                if (hana != null)
                {
                    context.ExamAttempts.AddRange(
                        new ExamAttempt
                        {
                            UserId = hana.Id, ExamId = exams[0].Id,
                            StartedAt = DateTime.UtcNow.AddDays(-18), CompletedAt = DateTime.UtcNow.AddDays(-18).AddMinutes(110),
                            ListeningScore = 300, ReadingScore = 270, TotalScore = 570, IsSubmitted = true
                        },
                        new ExamAttempt
                        {
                            UserId = hana.Id, ExamId = exams[1].Id,
                            StartedAt = DateTime.UtcNow.AddDays(-10), CompletedAt = DateTime.UtcNow.AddDays(-10).AddMinutes(105),
                            ListeningScore = 340, ReadingScore = 310, TotalScore = 650, IsSubmitted = true
                        },
                        new ExamAttempt
                        {
                            UserId = hana.Id, ExamId = exams[2].Id,
                            StartedAt = DateTime.UtcNow.AddDays(-3), CompletedAt = DateTime.UtcNow.AddDays(-3).AddMinutes(100),
                            ListeningScore = 370, ReadingScore = 340, TotalScore = 710, IsSubmitted = true
                        }
                    );
                }

                await context.SaveChangesAsync();
            }
        }

        // ── 7.5 AttemptAnswers (for Skill Analytics) ──
        if (!context.AttemptAnswers.Any())
        {
            var seededAttempts = context.ExamAttempts.ToList();
            var questions = context.Questions.ToList();
            var random = new Random(42);

            foreach (var attempt in seededAttempts)
            {
                var examQuestions = questions.Where(q => q.ExamId == attempt.ExamId).ToList();
                foreach(var q in examQuestions)
                {
                    // Mock varying correctness to show diverse Radar charts
                    // Part 1, 2, 5 generally higher. Part 3, 4, 7 lower.
                    double threshold = 0.5;
                    if (q.Part == 1 || q.Part == 2 || q.Part == 5) threshold = 0.2; // 80% correct
                    else if (q.Part == 7) threshold = 0.6; // 40% correct

                    bool isCorrect = random.NextDouble() > threshold;
                    context.AttemptAnswers.Add(new AttemptAnswer
                    {
                        AttemptId = attempt.Id,
                        QuestionId = q.Id,
                        SelectedOption = isCorrect ? OptionLetter.A : OptionLetter.B,
                        IsCorrect = isCorrect
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        // ── 8. StudyStreaks ──
        if (!context.StudyStreaks.Any())
        {
            var testUser = context.Users.FirstOrDefault(u => u.Email == "user@acadprep.com");
            var hana     = context.Users.FirstOrDefault(u => u.Email == "hana@acadprep.com");

            if (testUser != null)
            {
                context.StudyStreaks.Add(new StudyStreak
                {
                    UserId = testUser.Id,
                    CurrentStreak = 12,
                    MaxStreak = 15,
                    LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
                });
            }
            if (hana != null)
            {
                context.StudyStreaks.Add(new StudyStreak
                {
                    UserId = hana.Id,
                    CurrentStreak = 5,
                    MaxStreak = 8,
                    LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
                });
            }
            await context.SaveChangesAsync();
        }

        // ── 9. UserAchievements ──
        if (!context.UserAchievements.Any())
        {
            var testUser     = context.Users.FirstOrDefault(u => u.Email == "user@acadprep.com");
            var achievements = context.Achievements.ToList();

            if (testUser != null && achievements.Count >= 3)
            {
                // Test User unlocked "First Blood" and "Streak Master"
                context.UserAchievements.AddRange(
                    new UserAchievement
                    {
                        UserId = testUser.Id,
                        AchievementId = achievements[0].AchievementId, // First Blood
                        UnlockedAt = DateTime.UtcNow.AddDays(-28)
                    },
                    new UserAchievement
                    {
                        UserId = testUser.Id,
                        AchievementId = achievements[2].AchievementId, // Streak Master
                        UnlockedAt = DateTime.UtcNow.AddDays(-5)
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
