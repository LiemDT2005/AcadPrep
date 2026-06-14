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
                _context.Users.Add(new User
                {
                    Email = "learner@test.com",
                    FullName = "Nguyen Van Learner",
                    PasswordHash = "hashedpassword", // Not real auth for seeding purposes
                    Status = UserStatus.Active,
                    RoleId = learnerRole.RoleId
                });
                await _context.SaveChangesAsync();
            }
        }

        // 3. Seed ExamSeries
        if (!_context.Set<ExamSeries>().Any())
        {
            _logger.LogInformation("Seeding ExamSeries...");
            _context.Set<ExamSeries>().AddRange(
                new ExamSeries { Name = "ETS TOEIC", Year = 2024, Description = "Bộ đề ETS 2024 mới nhất", CoverImageUrl = "https://lh3.googleusercontent.com/aida/AP1WRLv6grMpZ4SmtmiRm69Pd3l6wFn75kkTWccmwDmxsjsiUBng94tZYszb_nwwv4mAIBpzLVEC0jSQS_ccEvPkIShBd0C7wd3-IMgP3Js0VoZiBRRI4N4RTOcUBJc8LnbqTO-XjvWqOx3xrP7xLBQ8aKfoJ4dHGScY5oX_UBpseU5Nyhbpr4oKbbYv5RrhmLfMXMleOkHBTXVSxy4OtNvTkq6GZhK-xkMrmoH5xpsrUX16VsIUqp586Io9CYah" },
                new ExamSeries { Name = "Hacker TOEIC", Year = 2023, Description = "Bộ đề Hacker siêu khó", CoverImageUrl = "https://lh3.googleusercontent.com/aida/AP1WRLv6grMpZ4SmtmiRm69Pd3l6wFn75kkTWccmwDmxsjsiUBng94tZYszb_nwwv4mAIBpzLVEC0jSQS_ccEvPkIShBd0C7wd3-IMgP3Js0VoZiBRRI4N4RTOcUBJc8LnbqTO-XjvWqOx3xrP7xLBQ8aKfoJ4dHGScY5oX_UBpseU5Nyhbpr4oKbbYv5RrhmLfMXMleOkHBTXVSxy4OtNvTkq6GZhK-xkMrmoH5xpsrUX16VsIUqp586Io9CYah" }
            );
            await _context.SaveChangesAsync();
        }

        // 4. Seed Exams
        if (!_context.Exams.Any())
        {
            _logger.LogInformation("Seeding Exams...");
            var etsSeries = await _context.Set<ExamSeries>().FirstOrDefaultAsync(s => s.Name == "ETS TOEIC");
            var hackerSeries = await _context.Set<ExamSeries>().FirstOrDefaultAsync(s => s.Name == "Hacker TOEIC");

            if (etsSeries != null && hackerSeries != null)
            {
                _context.Exams.AddRange(
                    new Exam { Title = "ETS TOEIC 2024 - Test 1", Description = "Đề thi thử số 1 của bộ ETS 2024", Duration = 120, ExamSeriesId = etsSeries.Id },
                    new Exam { Title = "ETS TOEIC 2024 - Test 2", Description = "Đề thi thử số 2 của bộ ETS 2024", Duration = 120, ExamSeriesId = etsSeries.Id },
                    new Exam { Title = "Hacker TOEIC 3 - Test 1", Description = "Đề thi Hacker khó mục tiêu 800+", Duration = 120, ExamSeriesId = hackerSeries.Id }
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
