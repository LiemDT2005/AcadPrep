using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Courses.Queries.Common.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Courses.Queries.GetCourseList;

internal sealed class GetCourseListQueryHandler(IAppDbContext context, ICacheService cache)
    : IRequestHandler<GetCourseListQuery, Result<List<GetCourseDto>>>
{
    private const string CacheKey = "CourseList";

    public async Task<Result<List<GetCourseDto>>> Handle(
        GetCourseListQuery request, CancellationToken cancellationToken)
    {
        // Thử lấy từ Redis cache trước
        var cached = await cache.GetAsync<List<GetCourseDto>>(CacheKey, cancellationToken);
        if (cached is not null)
        {
            return Result<List<GetCourseDto>>.Success(cached);
        }

        // Cache miss → query DB
        var items = await context.Courses
            .Where(x => !x.IsDeleted && x.IsActive)
            .Select(x => new GetCourseDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Level = x.Level,
                Price = x.Price,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Lưu vào cache (sliding 5 phút)
        await cache.SetAsync(CacheKey, items, TimeSpan.FromMinutes(5), cancellationToken);  

        return Result<List<GetCourseDto>>.Success(items);
    }
}
