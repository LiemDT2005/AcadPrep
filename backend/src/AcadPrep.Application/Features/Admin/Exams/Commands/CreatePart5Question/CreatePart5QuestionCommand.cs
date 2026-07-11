using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreatePart5Question;

public class CreatePart5QuestionCommand : IRequest<Result<int>>
{
    public int ExamId { get; set; }
    public required Part5QuestionDto Question { get; set; }
}
