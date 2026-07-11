namespace AcadPrep.Application.Features.Admin.Exams.Commands.UpdateExam;

public class UpdateExamDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Duration { get; set; }
    public int ExamSeriesId { get; set; }
    public string? AudioUrl { get; set; }
}
