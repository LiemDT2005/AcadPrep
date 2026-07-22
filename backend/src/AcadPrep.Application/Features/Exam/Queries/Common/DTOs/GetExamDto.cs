namespace Application.Features.Exam.Queries.Common.DTOs;

public class GetExamDto
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required int Duration { get; set; }
    public required string SeriesName { get; set; }
    public required int Year { get; set; }  
    public required string CoverImageUrl { get; set; }
    public required int QuestionCount { get; set; }  
    public required int AttemptCount { get; set; }
    
}