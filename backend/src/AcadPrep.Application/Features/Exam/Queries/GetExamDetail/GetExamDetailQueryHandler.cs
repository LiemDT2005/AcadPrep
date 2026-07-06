using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using Application.Features.Exam.Queries.GetExamDetail;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Exam.Queries.GetExamDetail;

internal sealed class GetExamDetailQueryHandler(IAppDbContext context, ICacheService cache)
    : IRequestHandler<GetExamDetailQuery, Result<GetExamDetailDto>>
{
    public async Task<Result<GetExamDetailDto>> Handle(
        GetExamDetailQuery request, CancellationToken cancellationToken)
    {
        // 1. Tạo Cache Key động chứa cả UserId để tránh cache chéo lịch sử làm bài giữa các User
        var cacheKey = $"ExamDetail_{request.Id}_U_{request.UserId ?? 0}";

        // 2. Thử lấy dữ liệu từ Redis cache trước
        var cached = await cache.GetAsync<GetExamDetailDto>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return Result<GetExamDetailDto>.Success(cached);
        }

        // 3. Truy vấn thông tin đề thi cơ bản từ SQL Server
        var exam = await context.Exams
            .AsNoTracking()
            .Where(e => e.Id == request.Id && !e.IsDeleted)
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.Description,
                e.Duration,
                SeriesName = e.ExamSeries.Name,
                Year = e.ExamSeries.Year,
                CoverImageUrl = e.ExamSeries.CoverImageUrl
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (exam is null)
        {
            return Result<GetExamDetailDto>.Failure("Không tìm thấy đề thi hoặc đề thi đã bị xóa.");
        }

        // 4. Đếm tổng lượt làm bài của đề thi này
        var totalAttempts = await context.ExamAttempts
            .CountAsync(ea => ea.ExamId == request.Id, cancellationToken);

        // 5. Tải danh sách câu hỏi của đề thi này để thống kê theo từng Part (Part 1 - 7) và lấy các Tags
        var questionsOfExam = await context.Questions
            .AsNoTracking()
            .Where(q => q.ExamId == request.Id)
            .Select(q => new { q.Part, q.TopicTag })
            .ToListAsync(cancellationToken);

        var partsSummary = questionsOfExam
            .GroupBy(q => q.Part)
            .Select(g => new PartSummaryDto
            {
                PartNumber = g.Key,
                PartName = GetPartName(g.Key),
                QuestionCount = g.Count(),
                Tags = g.Where(q => !string.IsNullOrEmpty(q.TopicTag))
                        .Select(q => q.TopicTag!)
                        .Distinct()
                        .OrderBy(t => t)
                        .ToList()
            })
            .OrderBy(p => p.PartNumber)
            .ToList();

        var totalQuestions = partsSummary.Sum(p => p.QuestionCount);

        // 6. Khởi tạo đối tượng DTO kết quả
        var examDetailDto = new GetExamDetailDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            Duration = exam.Duration,
            SeriesName = exam.SeriesName,
            Year = exam.Year,
            CoverImageUrl = exam.CoverImageUrl,
            TotalQuestions = totalQuestions,
            TotalAttempts = totalAttempts,
            Parts = partsSummary
        };

        // 7. Nếu User đã đăng nhập, lấy thêm lịch sử làm bài của họ cho đề thi này
        if (request.UserId.HasValue)
        {
            var history = await context.ExamAttempts
                .AsNoTracking()
                .Where(ea => ea.ExamId == request.Id && ea.UserId == request.UserId.Value)
                .OrderByDescending(ea => ea.StartedAt)
                .Select(ea => new UserAttemptHistoryDto
                {
                    AttemptId = ea.Id,
                    ListeningScore = ea.ListeningScore,
                    ReadingScore = ea.ReadingScore,
                    TotalScore = ea.TotalScore,
                    StartedAt = ea.StartedAt,
                    CompletedAt = ea.CompletedAt,
                    RemainingTime = ea.RemainingTime,
                    IsSubmitted = ea.IsSubmitted
                })
                .ToListAsync(cancellationToken);

            examDetailDto.AttemptHistory = history;
        }

        // 8. Lưu kết quả vào Redis cache (sliding 5 phút)
        await cache.SetAsync(cacheKey, examDetailDto, TimeSpan.FromMinutes(5), cancellationToken);

        return Result<GetExamDetailDto>.Success(examDetailDto);
    }

    // Hàm helper chuyển đổi số Part sang tên hiển thị của cấu trúc đề thi TOEIC
    private static string GetPartName(int partNumber)
    {
        return partNumber switch
        {
            1 => "Part 1: Photographs",
            2 => "Part 2: Question - Response",
            3 => "Part 3: Conversations",
            4 => "Part 4: Talks",
            5 => "Part 5: Incomplete Sentences",
            6 => "Part 6: Text Completion",
            7 => "Part 7: Reading Comprehension",
            _ => $"Part {partNumber}"
        };
    }
}
