using Application.Common.Models;
using MediatR;

namespace Application.Features.Exams.Commands.CreateExam;

public class CreateExamCommand : IRequest<Result<int>>
{
    public required CreateExamDto CreateExamDto { get; set; }
}
