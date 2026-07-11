using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Admin.Exams.Queries.Common.DTOs;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Queries.GetExamDetail;

public class GetExamDetailQuery : IRequest<Result<ExamDetailDto>>
{
    public int Id { get; set; }
}
