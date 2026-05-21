namespace Application.Features.Courses.Commands.CreateCourse;

public class CreateCourseDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Level { get; set; }
    public decimal Price { get; set; }
}
