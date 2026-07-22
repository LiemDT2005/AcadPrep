using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Admin.Exams.Queries.Common.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Exams.Queries.GetAdminExamList;

internal sealed class GetAdminExamListQueryHandler(IAppDbContext context)
    : IRequestHandler<GetAdminExamListQuery, Result<List<AdminExamDto>>>
{
    public async Task<Result<List<AdminExamDto>>> Handle(
        GetAdminExamListQuery request, CancellationToken cancellationToken)
    {
        // Sử dụng IgnoreQueryFilters để hiển thị cả các đề thi đã bị xóa mềm/ẩn
        var items = await context.Exams
            .IgnoreQueryFilters()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AdminExamDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Duration = x.Duration,
                IsDeleted = x.IsDeleted,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                AttemptCount = x.ExamAttempts.Count()
            })
            .ToListAsync(cancellationToken);

        return Result<List<AdminExamDto>>.Success(items);
    }
}
