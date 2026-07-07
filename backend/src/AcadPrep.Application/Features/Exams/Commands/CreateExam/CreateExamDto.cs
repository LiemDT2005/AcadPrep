namespace Application.Features.Exams.Commands.CreateExam;

public class CreateExamDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Duration { get; set; }
    public int ExamSeriesId { get; set; }
}
