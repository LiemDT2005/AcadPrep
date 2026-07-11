using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.RestoreExam;

public class RestoreExamCommand : IRequest<Result>
{
    public int Id { get; set; }
}
