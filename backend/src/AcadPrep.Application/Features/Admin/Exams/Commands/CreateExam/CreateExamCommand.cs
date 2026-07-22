using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.CreateExam;

public class CreateExamCommand : IRequest<Result<int>>
{
    public required CreateExamDto CreateExamDto { get; set; }
}
