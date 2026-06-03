using System.Threading.Tasks;
using Application.Features.Courses.Commands.CreateCourse;
using Application.Features.Courses.Queries.GetCourseList;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WebUI.Controllers;

public class CoursesController : ApiControllerBase
{
    /// <summary>
    /// Tạo một khóa học mới
    /// </summary>
    /// <param name="dto">Thông tin khóa học cần tạo</param>
    /// <returns>ID của khóa học vừa tạo</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/Courses
    ///     {
    ///        "title": "Tiếng Anh Giao Tiếp",
    ///        "description": "Khóa học cho người mới bắt đầu",
    ///        "level": "Beginner",
    ///        "price": 500000
    ///     }
    ///
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
    {
        var result = await Mediator.Send(new CreateCourseCommand { CreateCourseDto = dto });
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetList), new { id = result.Data }, result);
    }

    /// <summary>
    /// Lấy danh sách tất cả khóa học
    /// </summary>
    /// <returns>Danh sách khóa học</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
