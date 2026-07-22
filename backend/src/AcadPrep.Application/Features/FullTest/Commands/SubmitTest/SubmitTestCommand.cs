using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.FullTest.Commands.SubmitTest;

public record SubmitTestCommand(int AttemptId, int UserId, int? RemainingSeconds) : IRequest<Result<SubmitTestResultDto>>;

public class SubmitTestResultDto
{
    public int AttemptId { get; set; }
    public int ListeningScore { get; set; }
    public int ReadingScore { get; set; }
    public int TotalScore { get; set; }
    public int ListeningCorrect { get; set; }
    public int ReadingCorrect { get; set; }
}
