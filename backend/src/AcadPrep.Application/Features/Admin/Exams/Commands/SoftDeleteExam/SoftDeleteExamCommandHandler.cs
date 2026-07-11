using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.SoftDeleteExam;

internal sealed class SoftDeleteExamCommandHandler(IAppDbContext context)
    : IRequestHandler<SoftDeleteExamCommand, Result>
{
    public async Task<Result> Handle(SoftDeleteExamCommand request, CancellationToken cancellationToken)
    {
        // Sử dụng IgnoreQueryFilters để tìm kiếm bản ghi bất kể trạng thái IsDeleted
        var exam = await context.Exams
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (exam == null)
        {
            return Result.Failure("Exam not found.");
        }

        // Thực hiện xóa mềm/ẩn đề thi theo luật BR-15
        exam.SoftDelete();
        exam.LastModifiedAt = DateTime.UtcNow;

        var success = await context.SaveChangesAsync(cancellationToken) > 0;

        if (!success)
        {
            return Result.Failure("Could not hide/delete the exam.");
        }

        return Result.Success();
    }
}
