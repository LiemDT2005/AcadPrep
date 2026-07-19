using AcadPrep.Application.Common.Models;
using Domain.Enums;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.ChangeExamStatus;

public class ChangeExamStatusCommand : IRequest<Result>
{
    public int Id { get; set; }
    public ExamStatus Status { get; set; }
}
