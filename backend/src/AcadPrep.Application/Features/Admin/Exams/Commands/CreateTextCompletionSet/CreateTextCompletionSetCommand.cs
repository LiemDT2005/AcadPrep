using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateTextCompletionSet;

public class CreateTextCompletionSetCommand : IRequest<Result<int>>
{
    public int ExamId { get; set; }
    public required CreateTextCompletionSetDto Set { get; set; }
}
