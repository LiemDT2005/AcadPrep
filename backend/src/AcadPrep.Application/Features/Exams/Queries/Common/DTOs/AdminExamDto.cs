using System;

namespace Application.Features.Exams.Queries.Common.DTOs;

public class AdminExamDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Duration { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AttemptCount { get; set; }
}
