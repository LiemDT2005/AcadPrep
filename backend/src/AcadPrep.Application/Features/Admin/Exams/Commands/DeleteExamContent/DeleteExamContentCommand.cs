using System.Collections.Generic;
using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.DeleteExamContent;

public class DeleteExamContentCommand : IRequest<Result<DeleteExamContentResultDto>>
{
    public int ExamId { get; set; }
    public required string ContentType { get; set; } // "part1", "part2", "part5", "listening", "textcompletion", "readingset"
    public int TargetId { get; set; } // questionId, groupId (for listening/reading), or passageId (for part 6)
}

public class DeleteExamContentResultDto
{
    public List<int> DeletedQuestionIds { get; set; } = new();
    public List<int> DeletedPassageIds { get; set; } = new();
    public int? DeletedQuestionGroupId { get; set; }
}
