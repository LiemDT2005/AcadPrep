using System.Threading.Tasks;
using Application.Features.Courses.Commands.CreateCourse;
using Application.Features.Courses.Queries.GetCourseList;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

public class CoursesController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
    {
        var result = await Mediator.Send(new CreateCourseCommand { CreateCourseDto = dto });
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetList), new { id = result.Value }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var result = await Mediator.Send(new GetCourseListQuery());
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
