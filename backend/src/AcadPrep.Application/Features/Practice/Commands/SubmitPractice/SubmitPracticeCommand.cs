using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Practice.Commands.SubmitPractice;

public record SubmitPracticeCommand(
    int SessionId,
    int UserId,
    Dictionary<int, string> Answers) : IRequest<Result<SubmitPracticeResultDto>>;

public class SubmitPracticeResultDto
{
    public int SessionId { get; set; }
    public int ExamId { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public int ListeningCorrect { get; set; }
    public int ReadingCorrect { get; set; }
    public int ListeningTotal { get; set; }
    public int ReadingTotal { get; set; }
}
