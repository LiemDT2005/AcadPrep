using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public class AppDbContextInitializer
{
    private readonly ILogger<AppDbContextInitializer> _logger;
    private readonly AppDbContext _context;

    public AppDbContextInitializer(ILogger<AppDbContextInitializer> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Có lỗi xảy ra khi seed database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // 1. Seed Roles
        if (!_context.Roles.Any())
        {
            _logger.LogInformation("Seeding Roles...");
            _context.Roles.AddRange(
                new Role { RoleName = "Admin" },
                new Role { RoleName = "Learner" }
            );
            await _context.SaveChangesAsync();
        }

        // 2. Seed Users
        if (!_context.Users.Any())
        {
            _logger.LogInformation("Seeding Users...");
            var learnerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Learner");
            if (learnerRole != null)
            {
                var learnerUser = User.Create(
                    "learner@test.com",
                    "Nguyen Van Learner",
                    "hashedpassword", // Not real auth for seeding purposes
                    learnerRole.RoleId);
                learnerUser.Activate();
                _context.Users.Add(learnerUser);
                await _context.SaveChangesAsync();
            }
        }

        // 3. Seed ExamSeries
        if (!_context.Set<ExamSeries>().Any())
        {
            _logger.LogInformation("Seeding ExamSeries...");
            _context.Set<ExamSeries>().AddRange(
                new ExamSeries { Name = "ETS TOEIC", Year = 2024, Description = "The latest ETS 2024 practice test series", CoverImageUrl = "https://m.media-amazon.com/images/I/71wK8nN2nYL._AC_UF1000,1000_QL80_.jpg" },
                new ExamSeries { Name = "Hacker TOEIC", Year = 2023, Description = "Extremely difficult Hacker practice test series", CoverImageUrl = "https://bizweb.dktcdn.net/100/413/851/products/22-ae9a0dc1-93db-4e78-9e6b-67a659cc60ff.jpg?v=1626245084930" },
                new ExamSeries { Name = "New Economy TOEIC", Year = 2023, Description = "Economy series that closely matches the actual exam", CoverImageUrl = "https://bizweb.dktcdn.net/100/413/851/products/21-82d2719a-9e17-48f8-bba9-0f04e18cc8cf.jpg?v=1626245085443" },
                new ExamSeries { Name = "YBM TOEIC", Year = 2025, Description = "The newly released YBM TOEIC Vol 3 practice series", CoverImageUrl = "https://bizweb.dktcdn.net/100/413/851/products/25-27a9cfd4-bc31-4171-8857-79774ccbe0c5.jpg?v=1626245086207" }
            );
            await _context.SaveChangesAsync();
        }

        // 4. Seed Exams
        if (!_context.Exams.Any())
        {
            _logger.LogInformation("Seeding Exams...");
            var etsSeries = await _context.Set<ExamSeries>().FirstOrDefaultAsync(s => s.Name == "ETS TOEIC");
            var hackerSeries = await _context.Set<ExamSeries>().FirstOrDefaultAsync(s => s.Name == "Hacker TOEIC");
            var ecoSeries = await _context.Set<ExamSeries>().FirstOrDefaultAsync(s => s.Name == "New Economy TOEIC");
            var ybmSeries = await _context.Set<ExamSeries>().FirstOrDefaultAsync(s => s.Name == "YBM TOEIC");

            if (etsSeries != null && hackerSeries != null && ecoSeries != null && ybmSeries != null)
{
    _context.Exams.AddRange(
        new Exam
        {
            Title = "ETS TOEIC 2024 - Test 1",
            Description = "Practice Test 1 from the ETS TOEIC 2024 series.",
            Duration = 120,
            ExamSeriesId = etsSeries.Id
        },
        new Exam
        {
            Title = "ETS TOEIC 2024 - Test 2",
            Description = "Practice Test 2 from the ETS TOEIC 2024 series.",
            Duration = 120,
            ExamSeriesId = etsSeries.Id
        },
        new Exam
        {
            Title = "ETS TOEIC 2024 - Test 3",
            Description = "Practice Test 3 from the ETS TOEIC 2024 series.",
            Duration = 120,
            ExamSeriesId = etsSeries.Id
        },
        new Exam
        {
            Title = "ETS TOEIC 2024 - Test 4",
            Description = "Practice Test 4 from the ETS TOEIC 2024 series.",
            Duration = 120,
            ExamSeriesId = etsSeries.Id
        },
        new Exam
        {
            Title = "ETS TOEIC 2024 - Test 5",
            Description = "Practice Test 5 from the ETS TOEIC 2024 series.",
            Duration = 120,
            ExamSeriesId = etsSeries.Id
        },

        new Exam
        {
            Title = "Hacker TOEIC 3 - Test 1",
            Description = "A challenging practice test designed for learners aiming for a TOEIC score of 800+.",
            Duration = 120,
            ExamSeriesId = hackerSeries.Id
        },
        new Exam
        {
            Title = "Hacker TOEIC 3 - Test 2",
            Description = "A challenging practice test designed for learners aiming for a TOEIC score of 800+.",
            Duration = 120,
            ExamSeriesId = hackerSeries.Id
        },
        new Exam
        {
            Title = "Hacker TOEIC 3 - Test 3",
            Description = "A challenging practice test designed for learners aiming for a TOEIC score of 800+.",
            Duration = 120,
            ExamSeriesId = hackerSeries.Id
        },

        new Exam
        {
            Title = "New Economy TOEIC Vol. 1 - Test 1",
            Description = "An easy-level practice test closely matching the actual TOEIC exam.",
            Duration = 120,
            ExamSeriesId = ecoSeries.Id
        },
        new Exam
        {
            Title = "New Economy TOEIC Vol. 1 - Test 2",
            Description = "An intermediate-level practice test closely matching the actual TOEIC exam.",
            Duration = 120,
            ExamSeriesId = ecoSeries.Id
        },
        new Exam
        {
            Title = "New Economy TOEIC Vol. 1 - Test 3",
            Description = "An advanced-level practice test closely matching the actual TOEIC exam.",
            Duration = 120,
            ExamSeriesId = ecoSeries.Id
        },

        new Exam
        {
            Title = "YBM TOEIC Vol. 3 - Mock Test 1",
            Description = "A mock test based on the latest TOEIC exam format.",
            Duration = 120,
            ExamSeriesId = ybmSeries.Id
        },
        new Exam
        {
            Title = "YBM TOEIC Vol. 3 - Mock Test 2",
            Description = "A mock test based on the latest TOEIC exam format.",
            Duration = 120,
            ExamSeriesId = ybmSeries.Id
        },
        new Exam
        {
            Title = "YBM TOEIC Vol. 3 - Mock Test 3",
            Description = "A mock test based on the latest TOEIC exam format.",
            Duration = 120,
            ExamSeriesId = ybmSeries.Id
        }
    );

    await _context.SaveChangesAsync();
}
        }

        // 5. Seed Questions for ETS 2024 - Test 1
        if (!_context.Questions.Any())
        {
            _logger.LogInformation("Seeding Questions...");
            var exam1 = await _context.Exams.FirstOrDefaultAsync(e => e.Title.Contains("ETS TOEIC 2024 - Test 1"));
            if (exam1 != null)
            {
                var questions = new List<Question>();
                var rnd = new Random();
                
                // Add mock questions for Part 1 to 7
                int qNum = 1;
                var partCounts = new[] { 6, 25, 39, 30, 30, 16, 54 }; // TOEIC standard question counts
                
                for (int part = 1; part <= 7; part++)
                {
                    int countForPart = partCounts[part - 1];
                    for (int i = 0; i < countForPart; i++)
                    {
                        questions.Add(new Question
                        {
                            ExamId = exam1.Id,
                            Part = part,
                            QuestionNumber = qNum++,
                            QuestionText = $"This is mock question {qNum} for Part {part}",
                            CorrectOption = (OptionLetter)rnd.Next(0, 4)
                        });
                    }
                }
                
                _context.Questions.AddRange(questions);
                await _context.SaveChangesAsync();
            }
        }

        // 6. Seed ExamAttempts (Lịch sử làm bài)
        if (!_context.ExamAttempts.Any())
        {
            _logger.LogInformation("Seeding Exam Attempts...");
            var learner = await _context.Users.FirstOrDefaultAsync(u => u.Email == "learner@test.com");
            var exam1 = await _context.Exams.FirstOrDefaultAsync(e => e.Title.Contains("ETS TOEIC 2024 - Test 1"));

            if (learner != null && exam1 != null)
            {
                _context.ExamAttempts.AddRange(
                    new ExamAttempt 
                    { 
                        UserId = learner.Id, 
                        ExamId = exam1.Id, 
                        StartedAt = DateTime.UtcNow.AddDays(-5), 
                        CompletedAt = DateTime.UtcNow.AddDays(-5).AddHours(2), 
                        IsSubmitted = true, 
                        ListeningScore = 350, 
                        ReadingScore = 300, 
                        TotalScore = 650, 
                        RemainingTime = 0 
                    },
                    new ExamAttempt 
                    { 
                        UserId = learner.Id, 
                        ExamId = exam1.Id, 
                        StartedAt = DateTime.UtcNow.AddDays(-1), 
                        IsSubmitted = false, 
                        ListeningScore = 0, 
                        ReadingScore = 0, 
                        TotalScore = 0, 
                        RemainingTime = 3600 // 1 hour left
                    }
                );
                await _context.SaveChangesAsync();
            }
        }
    }
}
