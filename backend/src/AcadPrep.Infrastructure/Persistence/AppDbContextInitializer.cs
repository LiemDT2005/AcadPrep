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
    private const string SampleListeningAudioUrl = "/audio/sample-listening.wav";
    private const int ListeningSegmentSeconds = 8;

    private readonly ILogger<AppDbContextInitializer> _logger;
    private readonly AppDbContext _context;
    private readonly Application.Common.Interfaces.IPasswordHasher _passwordHasher;

    public AppDbContextInitializer(ILogger<AppDbContextInitializer> logger, AppDbContext context, Application.Common.Interfaces.IPasswordHasher passwordHasher)
    {
        _logger = logger;
        _context = context;
        _passwordHasher = passwordHasher;
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
                new Role { RoleName = nameof(UserRole.Admin) },
                new Role { RoleName = nameof(UserRole.Learner) },
                new Role { RoleName = nameof(UserRole.Moderator) }
            );
            await _context.SaveChangesAsync();
        }

        // 2. Seed Users
        if (!_context.Users.Any())
        {
            _logger.LogInformation("Seeding Users...");
            var learnerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == nameof(UserRole.Learner));
            var moderatorRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == nameof(UserRole.Moderator));
            
            if (learnerRole != null)
            {
                var learnerUser = User.Create(
                    "learner@test.com",
                    "Nguyen Van Learner",
                    _passwordHasher.Hash("Password123!"),
                    learnerRole.RoleId,
                    DateTime.UtcNow);
                learnerUser.Activate();
                _context.Users.Add(learnerUser);
            }

            if (moderatorRole != null)
            {
                var moderatorUser = User.Create(
                    "moderator@test.com",
                    "Nguyen Van Moderator",
                    _passwordHasher.Hash("Password123!"),
                    moderatorRole.RoleId,
                    DateTime.UtcNow);
                moderatorUser.Activate();
                _context.Users.Add(moderatorUser);
            }

            await _context.SaveChangesAsync();
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

        // 4.5 Link exams missing a series (e.g. seeded by AppDbContextSeed)
        var defaultSeries = await _context.Set<ExamSeries>().FirstOrDefaultAsync();
        if (defaultSeries is not null)
        {
            var orphanExams = await _context.Exams
                .Where(e => e.ExamSeriesId == 0 || !_context.Set<ExamSeries>().Any(s => s.Id == e.ExamSeriesId))
                .ToListAsync();

            if (orphanExams.Count > 0)
            {
                _logger.LogInformation("Linking {Count} exam(s) to default series...", orphanExams.Count);
                foreach (var exam in orphanExams)
                {
                    exam.ExamSeriesId = defaultSeries.Id;
                }
                await _context.SaveChangesAsync();
            }
        }

        // 5. Seed Questions with answer options for exams that have none
        var examsNeedingQuestions = await _context.Exams
            .Where(e => !_context.Questions.Any(q => q.ExamId == e.Id))
            .OrderBy(e => e.Id)
            .Take(2)
            .ToListAsync();

        if (examsNeedingQuestions.Count > 0)
        {
            _logger.LogInformation("Seeding Questions for {Count} exam(s)...", examsNeedingQuestions.Count);
            foreach (var exam in examsNeedingQuestions)
            {
                exam.AudioUrl = SampleListeningAudioUrl;
                var questions = BuildToeicQuestions(exam.Id);
                _context.Questions.AddRange(questions);
            }

            await _context.SaveChangesAsync();
        }

        // 5.5 Backfill answer options for questions that have none
        var questionsMissingOptions = await _context.Questions
            .Include(q => q.QuestionOptions)
            .Where(q => !q.QuestionOptions.Any())
            .ToListAsync();

        if (questionsMissingOptions.Count > 0)
        {
            _logger.LogInformation("Backfilling options for {Count} question(s)...", questionsMissingOptions.Count);
            foreach (var question in questionsMissingOptions)
            {
                var optionTexts = BuildOptionTexts(question.Part, question.QuestionNumber);
                for (var i = 0; i < 4; i++)
                {
                    question.QuestionOptions.Add(new QuestionOption
                    {
                        OptionLetter = (OptionLetter)i,
                        OptionText = optionTexts[i]
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        // 5.6 Backfill listening audio for exams / questions missing media
        var examsMissingAudio = await _context.Exams
            .Where(e => e.AudioUrl == null && _context.Questions.Any(q => q.ExamId == e.Id && q.Part <= 4))
            .ToListAsync();

        if (examsMissingAudio.Count > 0)
        {
            _logger.LogInformation("Backfilling exam audio for {Count} exam(s)...", examsMissingAudio.Count);
            foreach (var exam in examsMissingAudio)
            {
                exam.AudioUrl = SampleListeningAudioUrl;
            }

            await _context.SaveChangesAsync();
        }

        var listeningQuestionsMissingAudio = await _context.Questions
            .Where(q => q.Part <= 4 && q.AudioUrl == null && q.AudioStartSecond == null)
            .OrderBy(q => q.ExamId)
            .ThenBy(q => q.QuestionNumber)
            .ToListAsync();

        if (listeningQuestionsMissingAudio.Count > 0)
        {
            _logger.LogInformation("Backfilling audio segments for {Count} listening question(s)...", listeningQuestionsMissingAudio.Count);
            var segmentIndexByExam = new Dictionary<int, int>();
            foreach (var question in listeningQuestionsMissingAudio)
            {
                if (!segmentIndexByExam.TryGetValue(question.ExamId, out var segmentIndex))
                {
                    segmentIndex = 0;
                }

                question.AudioStartSecond = segmentIndex * ListeningSegmentSeconds;
                question.AudioEndSecond = question.AudioStartSecond + ListeningSegmentSeconds;
                segmentIndexByExam[question.ExamId] = segmentIndex + 1;
            }

            await _context.SaveChangesAsync();
        }

        // 6. Seed ExamAttempts (Lịch sử làm bài)
        if (!_context.ExamAttempts.Any())
        {
            _logger.LogInformation("Seeding Exam Attempts...");
            var learner = await _context.Users.FirstOrDefaultAsync(u => u.Email == "learner@test.com");
            var exam1 = await _context.Exams.FirstOrDefaultAsync(e => e.Title.Contains("ETS TOEIC 2024 - Test 1"))
                ?? await _context.Exams.OrderBy(e => e.Id).FirstOrDefaultAsync();

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

    private static List<Question> BuildToeicQuestions(int examId)
    {
        var questions = new List<Question>();
        var partCounts = new[] { 6, 25, 39, 30, 30, 16, 54 };
        var rnd = new Random(42);
        var qNum = 1;
        var listeningSegmentIndex = 0;

        for (var part = 1; part <= 7; part++)
        {
            var (questionType, topicTag) = GetQuestionMetadata(part);
            for (var i = 0; i < partCounts[part - 1]; i++)
            {
                var correct = (OptionLetter)rnd.Next(0, 4);
                int? audioStart = null;
                int? audioEnd = null;
                if (part <= 4)
                {
                    audioStart = listeningSegmentIndex * ListeningSegmentSeconds;
                    audioEnd = audioStart + ListeningSegmentSeconds;
                    listeningSegmentIndex++;
                }

                questions.Add(CreateQuestion(
                    examId,
                    part,
                    qNum++,
                    BuildQuestionText(part, i + 1),
                    BuildOptionTexts(part, i + 1),
                    correct,
                    questionType,
                    topicTag,
                    audioStart,
                    audioEnd));
            }
        }

        return questions;
    }

    private static Question CreateQuestion(
        int examId,
        int part,
        int questionNumber,
        string questionText,
        string[] optionTexts,
        OptionLetter correctOption,
        string questionType,
        string topicTag,
        int? audioStartSecond = null,
        int? audioEndSecond = null)
    {
        var question = new Question
        {
            ExamId = examId,
            Part = part,
            QuestionNumber = questionNumber,
            QuestionText = questionText,
            CorrectOption = correctOption,
            QuestionType = questionType,
            TopicTag = topicTag,
            AudioStartSecond = audioStartSecond,
            AudioEndSecond = audioEndSecond
        };

        for (var i = 0; i < 4; i++)
        {
            question.QuestionOptions.Add(new QuestionOption
            {
                OptionLetter = (OptionLetter)i,
                OptionText = optionTexts[i]
            });
        }

        return question;
    }

    private static string BuildQuestionText(int part, int index)
    {
        return part switch
        {
            1 => $"Look at the photograph marked number {index}. Which statement best describes the scene?",
            2 => $"Question {index}: Mark the best response to the statement or question.",
            3 => $"Questions {index}-{index + 2} refer to the following conversation. What is the main topic?",
            4 => $"Questions {index}-{index + 2} refer to the following talk. What is the announcement mainly about?",
            5 => $"Choose the word or phrase that best completes the sentence: The manager asked the team to _____ the report by Friday.",
            6 => $"Read the passage and choose the best word for blank {index}: Our company has expanded _____ into new markets this year.",
            7 => $"Read the following text and answer question {index}: What is the purpose of this message?",
            _ => $"Question {index}"
        };
    }

    private static string[] BuildOptionTexts(int part, int index)
    {
        return part switch
        {
            1 =>
            [
                "They are reviewing documents at a table.",
                "They are waiting for a train at the station.",
                "They are planting trees in a garden.",
                "They are swimming in a pool."
            ],
            2 =>
            [
                "At 3 o'clock in the afternoon.",
                "Yes, I have finished the report.",
                "About twenty people attended.",
                "The meeting room on the third floor."
            ],
            3 or 4 =>
            [
                "Scheduling a business trip",
                "Ordering office supplies",
                "Repairing a computer",
                "Planning a company picnic"
            ],
            5 =>
            [
                "submit",
                "submitted",
                "submitting",
                "submits"
            ],
            6 =>
            [
                "rapidly",
                "rapid",
                "rapidity",
                "rapidness"
            ],
            7 =>
            [
                "To confirm a reservation",
                "To request a refund",
                "To advertise a product",
                "To announce a policy change"
            ],
            _ =>
            [
                $"Option A for question {index}",
                $"Option B for question {index}",
                $"Option C for question {index}",
                $"Option D for question {index}"
            ]
        };
    }

    private static (string QuestionType, string TopicTag) GetQuestionMetadata(int partNumber)
    {
        return partNumber switch
        {
            1 => ("Photographs", "People at work"),
            2 => ("Question - Response", "WH-questions"),
            3 => ("Conversations", "Business travel"),
            4 => ("Talks", "Public announcements"),
            5 => ("Incomplete Sentences", "Verb tenses"),
            6 => ("Text Completion", "Passage completion"),
            7 => ("Reading Comprehension", "Email messages"),
            _ => ("General", "Other")
        };
    }
}
