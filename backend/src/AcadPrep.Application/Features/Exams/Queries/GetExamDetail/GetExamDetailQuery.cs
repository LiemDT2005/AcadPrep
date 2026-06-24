using Application.Common.Models;
using Application.Features.Exams.Queries.Common.DTOs;
using MediatR;

namespace Application.Features.Exams.Queries.GetExamDetail;

public class GetExamDetailQuery : IRequest<Result<ExamDetailDto>>
{
    public int Id { get; set; }
}
