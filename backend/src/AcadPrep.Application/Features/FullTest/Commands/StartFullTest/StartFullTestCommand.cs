using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.FullTest.Commands.StartFullTest;

public record StartFullTestCommand(int ExamId, int UserId) : IRequest<Result<StartFullTestResultDto>>;

public class StartFullTestResultDto
{
    public int AttemptId { get; set; }
    public bool IsResume { get; set; }
}
