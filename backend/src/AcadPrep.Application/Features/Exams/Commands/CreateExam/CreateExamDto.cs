using System.Collections.Generic;

namespace Application.Features.Exams.Commands.CreateExam;

public class CreateExamDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Duration { get; set; }
    public List<CreateQuestionDto> Questions { get; set; } = new();
}

public class CreateQuestionDto
{
    public int QuestionNumber { get; set; }
    public int Part { get; set; }
    public string? QuestionText { get; set; }
    public string? AudioUrl { get; set; }
    public required string CorrectOption { get; set; } // "A", "B", "C", "D"
    public string? PassageContent { get; set; }
    public required string OptionA { get; set; }
    public required string OptionB { get; set; }
    public required string OptionC { get; set; }
    public required string OptionD { get; set; }
}
