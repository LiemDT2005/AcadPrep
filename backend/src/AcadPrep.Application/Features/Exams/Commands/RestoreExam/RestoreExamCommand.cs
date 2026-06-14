using Application.Common.Models;
using MediatR;

namespace Application.Features.Exams.Commands.RestoreExam;

public class RestoreExamCommand : IRequest<Result>
{
    public int Id { get; set; }
}
