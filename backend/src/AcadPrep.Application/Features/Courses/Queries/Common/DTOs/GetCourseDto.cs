using System;

namespace Application.Features.Courses.Queries.Common.DTOs;

public class GetCourseDto
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Level { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set; }
}
