using AcadPrep.Application.Common.Caching;
using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using Application.Features.Exam.Queries.Common.DTOs;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Exam.Queries.GetExamList;

internal sealed class GetExamListQueryHandler(IAppDbContext context, ICacheService cache) 
    : IRequestHandler<GetExamListQuery, Result<GetExamListResponse>>
{
    public async Task<Result<GetExamListResponse>> Handle(GetExamListQuery request,
        CancellationToken cancellationToken)
    {
        var listVersion = await cache.GetVersionAsync(CacheKeys.ExamListVersion, cancellationToken);
        var cacheKey = $"ExamList_v{listVersion}_S:{request.Search ?? ""}_Sr:{request.SeriesName ?? "All"}_Y:{request.Year?.ToString() ?? "All"}_P:{request.PageIndex}_Sz:{request.PageSize}";

        var cached = await cache.GetAsync<GetExamListResponse>(cacheKey, cancellationToken);
        //Cache hit
        if (cached is not null)
        {
            return Result<GetExamListResponse>.Success(cached);
        }
        
        //Cache miss
        
        // Load dynamic filter options from ExamSeries entity
        var seriesNames = await context.ExamSeries
            .Where(s => !s.IsDeleted)
            .Select(s => s.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync(cancellationToken);

        var years = await context.ExamSeries
            .Where(s => !s.IsDeleted)
            .Select(s => s.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .Select(y => y.ToString())
            .ToListAsync(cancellationToken);

        var seriesFilters = new List<string> { "All" };
        seriesFilters.AddRange(seriesNames);

        var yearFilters = new List<string> { "All" };
        yearFilters.AddRange(years);

        var query = context.Exams.AsNoTracking()
            .Where(e => !e.IsDeleted && e.Status == ExamStatus.Published);
        
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

        var response = new GetExamListResponse
        {
            Exams = paginatedResult,
            SeriesFilters = seriesFilters,
            YearFilters = yearFilters
        };

        await cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);
        
        return Result<GetExamListResponse>.Success(response);
    }
}