using System.Collections.Generic;
using AcadPrep.Application.Features.Admin.Exams.Commands.CreateReadingSet;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateReadingSet;

public class UpdateReadingSetDto
{
    public required string Name { get; set; }
    public string? Explanation { get; set; }
    public List<UpdateReadingPassageDto> Passages { get; set; } = new();
    public List<UpdateReadingQuestionDto> Questions { get; set; } = new();
}

public class UpdateReadingPassageDto
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateReadingQuestionDto
{
    public int Id { get; set; }
    public int QuestionNumber { get; set; }
    public required string QuestionText { get; set; }
    public required string CorrectOption { get; set; }
    public List<ReadingOptionDto> Options { get; set; } = new();
}
