using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateListeningGroup;

public class UpdateListeningGroupCommand : IRequest<Result<int>>
{
    public int ExamId { get; set; }
    public int QuestionGroupId { get; set; }
    public required UpdateListeningGroupDto Group { get; set; }
}
