using Application.Common.Models;
using MediatR;

namespace Application.Features.Exam.Queries.GetExamDetail;

public record GetExamDetailQuery(int Id, int? UserId) : IRequest<Result<GetExamDetailDto>>;