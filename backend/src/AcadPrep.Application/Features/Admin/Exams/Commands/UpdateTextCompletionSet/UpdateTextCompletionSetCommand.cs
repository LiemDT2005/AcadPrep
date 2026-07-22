using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateTextCompletionSet;

public class UpdateTextCompletionSetCommand : IRequest<Result<int>>
{
    public int ExamId { get; set; }
    public int PassageId { get; set; }
    public required UpdateTextCompletionSetDto Set { get; set; }
}
