using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Caching;
using AcadPrep.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.RestoreExam;

internal sealed class RestoreExamCommandHandler(IAppDbContext context, ICacheService cache)
    : IRequestHandler<RestoreExamCommand, Result>
{
    public async Task<Result> Handle(RestoreExamCommand request, CancellationToken cancellationToken)
    {
        // Tìm kiếm đề thi kể cả đã bị xóa mềm/ẩn
        var exam = await context.Exams
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (exam == null)
        {
            return Result.Failure("Exam not found.");
        }

        // Khôi phục đề thi về trạng thái hoạt động bình thường
        exam.IsDeleted = false;
        exam.LastModifiedAt = DateTime.UtcNow;

        var success = await context.SaveChangesAsync(cancellationToken) > 0;

        if (!success)
        {
            return Result.Failure("Could not restore the exam.");
        }

        await cache.BumpVersionAsync(CacheKeys.ExamListVersion, cancellationToken);
        await cache.BumpVersionAsync(CacheKeys.ExamDetailVersion(exam.Id), cancellationToken);

        return Result.Success();
    }
}
