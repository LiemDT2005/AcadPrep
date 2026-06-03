using System;

namespace Application.Features.Courses.Queries.Common.DTOs;

public class GetCourseDto
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Level { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
