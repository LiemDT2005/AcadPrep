using System.Collections.Generic;

namespace Application.Features.Exams.Queries.Common.DTOs;

public class ReadingSetDto
{
    public int QuestionGroupId { get; set; }
    public required string Name { get; set; }
    public List<PassageDetailDto> Passages { get; set; } = new();
    public List<QuestionDetailDto> Questions { get; set; } = new();
}

public class PassageDetailDto
{
    public int Id { get; set; }
    public int DisplayOrder { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
}
