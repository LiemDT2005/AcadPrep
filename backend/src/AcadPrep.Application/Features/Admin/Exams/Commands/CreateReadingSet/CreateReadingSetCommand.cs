using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateReadingSet;

public class CreateReadingSetCommand : IRequest<Result<int>>
{
    public int ExamId { get; set; }
    public required CreateReadingSetDto Set { get; set; }
}
