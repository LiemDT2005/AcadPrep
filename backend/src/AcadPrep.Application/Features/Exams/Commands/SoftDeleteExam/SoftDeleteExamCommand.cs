using AcadPrep.Application.Common.Models;
using MediatR;

namespace Application.Features.Exams.Commands.SoftDeleteExam;

public class SoftDeleteExamCommand : IRequest<Result>
{
    public int Id { get; set; }
}
