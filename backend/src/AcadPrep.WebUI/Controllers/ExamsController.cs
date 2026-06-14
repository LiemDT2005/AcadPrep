using System.Threading.Tasks;
using Application.Features.Exams.Commands.CreateExam;
using Application.Features.Exams.Commands.SoftDeleteExam;
using Application.Features.Exams.Commands.RestoreExam;
using Application.Features.Exams.Queries.GetAdminExamList;
using Application.Features.Exams.Queries.GetExamDetail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WebUI.Controllers;

public class ExamsController : ApiControllerBase
{
    /// <summary>
    /// Lấy danh sách tất cả đề thi quản trị (bao gồm cả đề đã ẩn)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetList()
    {
        var result = await Mediator.Send(new GetAdminExamListQuery());
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết cấu trúc đề thi, danh sách câu hỏi và lượt thi
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id)
    {
        var result = await Mediator.Send(new GetExamDetailQuery { Id = id });
        
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Tạo một đề thi mới kèm theo danh sách các câu hỏi
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateExamDto dto)
    {
        var result = await Mediator.Send(new CreateExamCommand { CreateExamDto = dto });
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetDetail), new { id = result.Data }, result);
    }

    /// <summary>
    /// Xóa mềm/Ẩn một đề thi khỏi hệ thống học tập công khai
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var result = await Mediator.Send(new SoftDeleteExamCommand { Id = id });
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Khôi phục một đề thi đã bị ẩn
    /// </summary>
    [HttpPost("{id:int}/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Restore(int id)
    {
        var result = await Mediator.Send(new RestoreExamCommand { Id = id });
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
