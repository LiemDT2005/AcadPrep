using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Caching;
using AcadPrep.Application.Common.Models;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.ChangeExamStatus;

internal sealed class ChangeExamStatusCommandHandler(IAppDbContext context, ICacheService cache)
    : IRequestHandler<ChangeExamStatusCommand, Result>
{
    public async Task<Result> Handle(ChangeExamStatusCommand request, CancellationToken cancellationToken)
    {
        // IgnoreQueryFilters để lấy được bản ghi bất kể trạng thái IsDeleted
        var exam = await context.Exams
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (exam is null)
        {
            return Result.Failure("Exam not found.");
        }

        if (exam.IsDeleted)
        {
            return Result.Failure("This exam is hidden. Please restore it before changing its status.");
        }

        if (exam.Status == request.Status)
        {
            return Result.Success();
        }

        // Không cho publish đề thi chưa có câu hỏi
        if (request.Status == ExamStatus.Published)
        {
            var hasQuestions = await context.Questions
                .AnyAsync(q => q.ExamId == exam.Id, cancellationToken);

            if (!hasQuestions)
            {
                return Result.Failure("Cannot publish an exam that has no questions yet.");
            }
        }

        exam.Status = request.Status;
        exam.LastModifiedAt = DateTime.UtcNow;

        var success = await context.SaveChangesAsync(cancellationToken) > 0;

        if (!success)
        {
            return Result.Failure("Could not update the exam status.");
        }

        // Vô hiệu hóa cache danh sách và chi tiết để phản ánh ngay lập tức
        await cache.BumpVersionAsync(CacheKeys.ExamListVersion, cancellationToken);
        await cache.BumpVersionAsync(CacheKeys.ExamDetailVersion(exam.Id), cancellationToken);

        return Result.Success();
    }
}
