using System.Collections.Generic;
using AcadPrep.Application.Features.Admin.Exams.Commands.CreateListeningGroup;

namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateListeningGroup;

public class UpdateListeningGroupDto
{
    public int Part { get; set; }
    public required string Name { get; set; }
    public ListeningGroupMediaDto Media { get; set; } = new();
    public List<UpdateListeningGroupQuestionDto> Questions { get; set; } = new();
}

public class UpdateListeningGroupQuestionDto
{
    public int Id { get; set; }
    public int QuestionNumber { get; set; }
    public string? QuestionText { get; set; }
    public string? ImageUrl { get; set; }
    public required string CorrectOption { get; set; }
    public List<ListeningOptionDto> Options { get; set; } = new();
}
