using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateReadingSet;

public class UpdateReadingSetCommand : IRequest<Result<int>>
{
    public int ExamId { get; set; }
    public int QuestionGroupId { get; set; }
    public required UpdateReadingSetDto Set { get; set; }
}
