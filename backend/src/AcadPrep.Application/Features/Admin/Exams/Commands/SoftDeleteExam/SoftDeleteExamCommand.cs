using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.SoftDeleteExam;

public class SoftDeleteExamCommand : IRequest<Result>
{
    public int Id { get; set; }
}
