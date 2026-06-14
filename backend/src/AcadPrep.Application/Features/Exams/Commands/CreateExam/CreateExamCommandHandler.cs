using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Exams.Commands.CreateExam;

internal sealed class CreateExamCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateExamCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateExamCommand request, CancellationToken cancellationToken)
    {
        var dto = request.CreateExamDto;

        // Khởi tạo thực thể Exam
        var exam = new Exam
        {
            Title = dto.Title,
            Description = dto.Description,
            Duration = dto.Duration,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        // Cache lưu các đoạn văn đã tạo để chia sẻ giữa các câu hỏi cùng đoạn văn
        var passageCache = new Dictionary<string, Passage>();

        if (dto.Questions != null && dto.Questions.Any())
        {
            foreach (var qDto in dto.Questions)
            {
                // Phân tích đáp án đúng sang enum OptionLetter
                if (!Enum.TryParse<OptionLetter>(qDto.CorrectOption, out var correctOptionEnum))
                {
                    correctOptionEnum = OptionLetter.A;
                }

                var question = new Question
                {
                    QuestionNumber = qDto.QuestionNumber,
                    Part = qDto.Part,
                    QuestionText = qDto.QuestionText,
                    AudioUrl = qDto.AudioUrl,
                    CorrectOption = correctOptionEnum,
                    Exam = exam
                };

                // Xử lý liên kết Passage cho Part 6 & 7
                if ((qDto.Part == 6 || qDto.Part == 7) && !string.IsNullOrWhiteSpace(qDto.PassageContent))
                {
                    var cleanPassageContent = qDto.PassageContent.Trim();
                    if (passageCache.TryGetValue(cleanPassageContent, out var existingPassage))
                    {
                        question.Passage = existingPassage;
                    }
                    else
                    {
                        var newPassage = new Passage
                        {
                            Content = cleanPassageContent,
                            Exam = exam
                        };
                        passageCache[cleanPassageContent] = newPassage;
                        question.Passage = newPassage;
                    }
                }

                // Thêm 4 lựa chọn A, B, C, D
                question.QuestionOptions.Add(new QuestionOption { OptionLetter = OptionLetter.A, OptionText = qDto.OptionA, Question = question });
                question.QuestionOptions.Add(new QuestionOption { OptionLetter = OptionLetter.B, OptionText = qDto.OptionB, Question = question });
                question.QuestionOptions.Add(new QuestionOption { OptionLetter = OptionLetter.C, OptionText = qDto.OptionC, Question = question });
                question.QuestionOptions.Add(new QuestionOption { OptionLetter = OptionLetter.D, OptionText = qDto.OptionD, Question = question });

                exam.Questions.Add(question);
            }
        }

        context.Exams.Add(exam);
        
        var success = await context.SaveChangesAsync(cancellationToken) > 0;

        if (!success)
        {
            return Result<int>.Failure("Could not save the new exam to the database.");
        }

        return Result<int>.Success(exam.Id);
    }
}
