using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateExam;

public class UpdateExamCommand : IRequest<Result<Unit>>
{
    public required UpdateExamDto UpdateExamDto { get; set; }
}
