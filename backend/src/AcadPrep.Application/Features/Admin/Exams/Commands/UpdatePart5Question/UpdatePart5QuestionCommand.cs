using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Admin.Exams.Commands.CreatePart5Question;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdatePart5Question;

public class UpdatePart5QuestionCommand : IRequest<Result<int>>
{
    public int ExamId { get; set; }
    public int QuestionId { get; set; }
    public required Part5QuestionDto Question { get; set; }
}
