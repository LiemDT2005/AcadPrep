using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Exam.Queries.Common.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Exam.Queries.GetExamList;

internal sealed class GetExamListQueryHandler(IAppDbContext context, ICacheService cache) 
    : IRequestHandler<GetExamListQuery, Result<PaginatedList<GetExamDto>>>
{
    public async Task<Result<PaginatedList<GetExamDto>>> Handle(GetExamListQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"ExamList_S:{request.Search ?? ""}_Sr:{request.SeriesName ?? "All"}_Y:{request.Year?.ToString() ?? "All"}_P:{request.PageIndex}_Sz:{request.PageSize}";

        var cached = await cache.GetAsync<PaginatedList<GetExamDto>>(cacheKey, cancellationToken);
        //Cache hit
        if (cached is not null)
        {
            return Result<PaginatedList<GetExamDto>>.Success(cached);
        }
        
        //Cache miss
        var query = context.Exams.AsNoTracking().Where(e => !e.IsDeleted);
        
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(e => e.Title.ToLower().Contains(searchLower));
        }
        
        if (!string.IsNullOrWhiteSpace(request.SeriesName))
        {
            query = query.Where(e => e.ExamSeries.Name == request.SeriesName);
        }
        
        if (request.Year.HasValue)
        {
            query = query.Where(e => e.ExamSeries.Year == request.Year.Value);
        }
        
        query = query.OrderByDescending(e => e.CreatedAt);
    
        var dtoQuery = query.Select(e => new GetExamDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Duration = e.Duration,
            SeriesName = e.ExamSeries.Name,
            Year = e.ExamSeries.Year,
            CoverImageUrl = e.ExamSeries.CoverImageUrl,
            QuestionCount = e.Questions.Count(),
            AttemptCount = e.ExamAttempts.Count()
        });
        
        var paginatedResult = await PaginatedList<GetExamDto>.CreateAsync(
            dtoQuery, 
            request.PageIndex, 
            request.PageSize
        );

        await cache.SetAsync(cacheKey, paginatedResult, TimeSpan.FromMinutes(5), cancellationToken);
        
        return Result<PaginatedList<GetExamDto>>.Success(paginatedResult);
    }
}