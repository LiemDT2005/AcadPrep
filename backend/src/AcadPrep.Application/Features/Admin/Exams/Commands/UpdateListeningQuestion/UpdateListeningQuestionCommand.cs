using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Admin.Exams.Commands.CreateListeningQuestion;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateListeningQuestion;

public class UpdateListeningQuestionCommand : IRequest<Result<int>>
{
    public int ExamId { get; set; }
    public int QuestionId { get; set; }
    public int Part { get; set; }
    public required ListeningQuestionInputDto Question { get; set; }
}
