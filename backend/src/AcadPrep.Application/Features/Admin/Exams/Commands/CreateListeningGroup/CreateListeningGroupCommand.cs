using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateListeningGroup;

public class CreateListeningGroupCommand : IRequest<Result<int>>
{
    public int ExamId { get; set; }
    public required CreateListeningGroupDto Group { get; set; }
}
