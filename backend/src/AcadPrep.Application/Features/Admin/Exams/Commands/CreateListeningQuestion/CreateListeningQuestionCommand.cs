using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateListeningQuestion;

public class CreateListeningQuestionCommand : IRequest<Result<int>>
{
    public int ExamId { get; set; }
    public int Part { get; set; }
    public required ListeningQuestionInputDto Question { get; set; }
}
