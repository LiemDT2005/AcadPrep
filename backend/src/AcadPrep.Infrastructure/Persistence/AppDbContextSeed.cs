using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence;

public static class AppDbContextSeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync();

        // ── 1. Roles ──
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { RoleName = nameof(UserRole.Admin) },
                new Role { RoleName = nameof(UserRole.Learner) },
                new Role { RoleName = nameof(UserRole.Moderator) }
            );
            await context.SaveChangesAsync();
        }

        // ── 2. Users ──
        if (!context.Users.Any())
        {
            var adminRole = context.Roles.First(r => r.RoleName == nameof(UserRole.Admin));
            var userRole  = context.Roles.First(r => r.RoleName == nameof(UserRole.Learner));
            var moderatorRole = context.Roles.First(r => r.RoleName == nameof(UserRole.Moderator));

            var now = DateTime.UtcNow;
            var defaultHash = passwordHasher.Hash("Password123!");
            // Sử dụng User.Create() factory vì User có private setters
            var adminUser   = User.Create("admin@acadprep.com",    "Admin User",    defaultHash, adminRole.RoleId, now); adminUser.Activate();
            var moderatorUser = User.Create("moderator@acadprep.com", "Moderator User", defaultHash, moderatorRole.RoleId, now); moderatorUser.Activate();
            var testUser    = User.Create("user@acadprep.com",     "Test User",     defaultHash, userRole.RoleId,  now);  testUser.Activate();
            var hanaUser    = User.Create("hana@acadprep.com",     "Hana Nguyen",   defaultHash, userRole.RoleId,  now);  hanaUser.Activate();
            var minhUser    = User.Create("minh@acadprep.com",     "Minh Tran",     defaultHash, userRole.RoleId,  now);  minhUser.Activate();
            // inactive user — không gọi Activate() → giữ nguyên Status = Inactive
            var inactiveUser = User.Create("inactive@acadprep.com", "Inactive User", defaultHash, userRole.RoleId, now);
            var hoangUser   = User.Create("hoang@acadprep.com",    "Hoang Nguyen",  defaultHash, userRole.RoleId,  now);  hoangUser.Activate();
            var lananhUser  = User.Create("lananh@acadprep.com",   "Lan Anh",       defaultHash, userRole.RoleId,  now);  lananhUser.Activate();
            var duyliemUser = User.Create("duyliem@acadprep.com",  "Duy Liem",      defaultHash, userRole.RoleId,  now);  duyliemUser.Activate();
            var tuananhUser = User.Create("tuananh@acadprep.com",  "Tuan Anh",      defaultHash, userRole.RoleId,  now);  tuananhUser.Activate();
            var thuthaoUser = User.Create("thuthao@acadprep.com",  "Thu Thao",      defaultHash, userRole.RoleId,  now);  thuthaoUser.Activate();
            var quanghuyUser= User.Create("quanghuy@acadprep.com", "Quang Huy",     defaultHash, userRole.RoleId,  now);  quanghuyUser.Activate();
            var bichUser    = User.Create("bichphuong@acadprep.com","Bich Phuong",  defaultHash, userRole.RoleId,  now);  bichUser.Activate();
            var namUser     = User.Create("hoangnam@acadprep.com", "Hoang Nam",     defaultHash, userRole.RoleId,  now);  namUser.Activate();
            var maiUser     = User.Create("maiphuong@acadprep.com","Mai Phuong",    defaultHash, userRole.RoleId,  now);  maiUser.Activate();
            var tungUser    = User.Create("thanhtung@acadprep.com","Thanh Tung",    defaultHash, userRole.RoleId,  now);  tungUser.Activate();

            var seededUsers = new[]
            {
                adminUser, moderatorUser, testUser, hanaUser, minhUser, inactiveUser,
                hoangUser, lananhUser, duyliemUser, tuananhUser, thuthaoUser,
                quanghuyUser, bichUser, namUser, maiUser, tungUser
            };

            foreach (var user in seededUsers)
            {
                user.UpdateAvatar(BuildSeedAvatarUrl(user.FullName));
            }

            context.Users.AddRange(seededUsers);
            await context.SaveChangesAsync();
        }

        // Backfill avatar cho database đã có dữ liệu từ các lần seed trước.
        var usersMissingAvatar = await context.Users
            .Where(user => user.AvatarUrl == null)
            .ToListAsync();
        foreach (var user in usersMissingAvatar)
        {
            user.UpdateAvatar(BuildSeedAvatarUrl(user.FullName));
        }
        if (usersMissingAvatar.Count > 0)
        {
            await context.SaveChangesAsync();
        }

        // Sửa hash mật khẩu cũ (không phải BCrypt) để login không bị 500.
        var repairedHash = passwordHasher.Hash("Password123!");
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE USERS SET PasswordHash = {repairedHash} WHERE PasswordHash IS NOT NULL AND PasswordHash NOT LIKE '$2%'");

        // ── 3. ExamSeries ──
        if (!context.ExamSeries.Any())
        {
            context.ExamSeries.AddRange(
                new ExamSeries { Name = "ETS",            Year = 2025, Description = "Official ETS TOEIC practice tests",                     CreatedAt = DateTime.UtcNow.AddDays(-60) },
                new ExamSeries { Name = "ETS",            Year = 2024, Description = "Official ETS TOEIC practice tests (2024 edition)",      CreatedAt = DateTime.UtcNow.AddDays(-120) },
                new ExamSeries { Name = "New Economy",    Year = 2025, Description = "Economy series – best-selling TOEIC prep in Vietnam",   CreatedAt = DateTime.UtcNow.AddDays(-55) },
                new ExamSeries { Name = "New Economy",    Year = 2024, Description = "Economy series – 2024 edition",                         CreatedAt = DateTime.UtcNow.AddDays(-110) },
                new ExamSeries { Name = "Hacker TOEIC",   Year = 2025, Description = "Advanced difficulty TOEIC practice",                    CreatedAt = DateTime.UtcNow.AddDays(-50) },
                new ExamSeries { Name = "Luyện Đề",       Year = 2025, Description = "Curated Vietnamese TOEIC practice collection",          CreatedAt = DateTime.UtcNow.AddDays(-45) }
            );
            await context.SaveChangesAsync();
        }

        // ── 4. Exams (20 exams across series) ──
        if (!context.Exams.Any())
        {
            var allSeries = context.ExamSeries.ToList();
            var ets2025       = allSeries.First(s => s.Name == "ETS"          && s.Year == 2025);
            var ets2024       = allSeries.First(s => s.Name == "ETS"          && s.Year == 2024);
            var economy2025   = allSeries.First(s => s.Name == "New Economy"  && s.Year == 2025);
            var economy2024   = allSeries.First(s => s.Name == "New Economy"  && s.Year == 2024);
            var hacker2025    = allSeries.First(s => s.Name == "Hacker TOEIC" && s.Year == 2025);
            var luyenDe2025   = allSeries.First(s => s.Name == "Luyện Đề"     && s.Year == 2025);

            context.Exams.AddRange(
                // ETS 2025 – 4 tests
                new Exam { Title = "ETS TOEIC 2025 Test 1",  Duration = 120, Description = "Official Practice Test 1 – Full simulation",               ExamSeriesId = ets2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-30) },
                new Exam { Title = "ETS TOEIC 2025 Test 2",  Duration = 120, Description = "Official Practice Test 2 – Listening focus",               ExamSeriesId = ets2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-28) },
                new Exam { Title = "ETS TOEIC 2025 Test 3",  Duration = 120, Description = "Official Practice Test 3 – Reading focus",                 ExamSeriesId = ets2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-26) },
                new Exam { Title = "ETS TOEIC 2025 Test 4",  Duration = 120, Description = "Official Practice Test 4 – Mixed difficulty",              ExamSeriesId = ets2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-24) },

                // ETS 2024 – 3 tests
                new Exam { Title = "ETS TOEIC 2024 Test 1",  Duration = 120, Description = "Previous year official test – excellent for baseline",      ExamSeriesId = ets2024.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-90) },
                new Exam { Title = "ETS TOEIC 2024 Test 2",  Duration = 120, Description = "Previous year official test – intermediate level",         ExamSeriesId = ets2024.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-85) },
                new Exam { Title = "ETS TOEIC 2024 Test 3",  Duration = 120, Description = "Previous year official test – advanced level",             ExamSeriesId = ets2024.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-80) },

                // New Economy 2025 – 4 tests
                new Exam { Title = "New Economy TOEIC Vol 7", Duration = 120, Description = "Latest Economy edition – trending practice set",           ExamSeriesId = economy2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-22) },
                new Exam { Title = "New Economy TOEIC Vol 8", Duration = 120, Description = "Economy series – comprehensive review",                    ExamSeriesId = economy2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-20) },
                new Exam { Title = "New Economy TOEIC Vol 9", Duration = 120, Description = "Economy series – Part 5-6-7 heavy",                       ExamSeriesId = economy2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-18) },
                new Exam { Title = "New Economy TOEIC Vol 10",Duration = 120, Description = "Economy series – final mock exam",                         ExamSeriesId = economy2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-16) },

                // New Economy 2024 – 3 tests
                new Exam { Title = "Economy TOEIC Vol 4",     Duration = 120, Description = "2024 Economy edition – Part 3-4 intensive",                ExamSeriesId = economy2024.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-75) },
                new Exam { Title = "Economy TOEIC Vol 5",     Duration = 120, Description = "2024 Economy edition – balanced difficulty",                ExamSeriesId = economy2024.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-70) },
                new Exam { Title = "Economy TOEIC Vol 6",     Duration = 120, Description = "2024 Economy edition – exam simulation",                   ExamSeriesId = economy2024.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-65) },

                // Hacker TOEIC 2025 – 3 tests
                new Exam { Title = "Hacker TOEIC Test 1",     Duration = 120, Description = "Hard difficulty – designed for 800+ target scorers",       ExamSeriesId = hacker2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-14) },
                new Exam { Title = "Hacker TOEIC Test 2",     Duration = 120, Description = "Hard difficulty – tricky grammar & vocabulary",            ExamSeriesId = hacker2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-12) },
                new Exam { Title = "Hacker TOEIC Test 3",     Duration = 120, Description = "Hard difficulty – long passage reading comprehension",     ExamSeriesId = hacker2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-10) },

                // Luyện Đề 2025 – 3 tests
                new Exam { Title = "Luyện Đề TOEIC Đề 1",    Duration = 120, Description = "Vietnamese curated practice – beginner friendly",          ExamSeriesId = luyenDe2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-8) },
                new Exam { Title = "Luyện Đề TOEIC Đề 2",    Duration = 120, Description = "Vietnamese curated practice – intermediate level",         ExamSeriesId = luyenDe2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-6) },
                new Exam { Title = "Luyện Đề TOEIC Đề 3",    Duration = 120, Description = "Vietnamese curated practice – full mock simulation",       ExamSeriesId = luyenDe2025.Id, Status = ExamStatus.Published, CreatedAt = DateTime.UtcNow.AddDays(-4) }
            );
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

                // Tạo lượt thi thử cho 10 học viên mới để có điểm xếp hạng
                var random = new Random();
                var newEmails = new[] { 
                    "hoang@acadprep.com", "lananh@acadprep.com", "duyliem@acadprep.com", 
                    "tuananh@acadprep.com", "thuthao@acadprep.com", "quanghuy@acadprep.com", 
                    "bichphuong@acadprep.com", "hoangnam@acadprep.com", "maiphuong@acadprep.com", 
                    "thanhtung@acadprep.com" 
                };
                foreach (var email in newEmails)
                {
                    var mockUser = context.Users.FirstOrDefault(u => u.Email == email);
                    if (mockUser != null)
                    {
                        int attemptCount = random.Next(1, 4); // 1 đến 3 lượt thi
                        for (int a = 0; a < attemptCount; a++)
                        {
                            int listen = random.Next(200, 451);
                            int read = random.Next(200, 451);
                            context.ExamAttempts.Add(new ExamAttempt
                            {
                                UserId = mockUser.Id,
                                ExamId = exams[random.Next(0, exams.Count)].Id,
                                StartedAt = DateTime.UtcNow.AddDays(-15 + a),
                                CompletedAt = DateTime.UtcNow.AddDays(-15 + a).AddMinutes(110),
                                ListeningScore = listen,
                                ReadingScore = read,
                                TotalScore = listen + read,
                                IsSubmitted = true
                            });
                        }
                    }
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

            // Thêm chuỗi ngày học cho 10 học viên mới để xếp hạng theo streak
            var randomStreak = new Random();
            var newEmails = new[] { 
                "hoang@acadprep.com", "lananh@acadprep.com", "duyliem@acadprep.com", 
                "tuananh@acadprep.com", "thuthao@acadprep.com", "quanghuy@acadprep.com", 
                "bichphuong@acadprep.com", "hoangnam@acadprep.com", "maiphuong@acadprep.com", 
                "thanhtung@acadprep.com" 
            };
            foreach (var email in newEmails)
            {
                var mockUser = context.Users.FirstOrDefault(u => u.Email == email);
                if (mockUser != null)
                {
                    int streak = randomStreak.Next(1, 15);
                    context.StudyStreaks.Add(new StudyStreak
                    {
                        UserId = mockUser.Id,
                        CurrentStreak = streak,
                        MaxStreak = streak + randomStreak.Next(0, 5),
                        LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-randomStreak.Next(0, 2)))
                    });
                }
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

    private static (string QuestionType, string TopicTag) GetQuestionMetadata(int partNumber)
    {
        return partNumber switch
        {
            1 => ("Photographs", "Tranh tả người"),
            2 => ("Question - Response", "Câu hỏi WHO"),
            3 => ("Conversations", "Chủ đề du lịch"),
            4 => ("Talks", "Thông báo công cộng"),
            5 => ("Incomplete Sentences", "Thì động từ"),
            6 => ("Text Completion", "Hoàn thành đoạn văn"),
            7 => ("Reading Comprehension", "Đọc hiểu email"),
            _ => ("General", "Khác")
        };
    }

    private static string BuildSeedAvatarUrl(string fullName)
    {
        return $"https://api.dicebear.com/9.x/initials/svg?seed={Uri.EscapeDataString(fullName)}";
    }
}

