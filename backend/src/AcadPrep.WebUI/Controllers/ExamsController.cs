using System.Threading.Tasks;
using Application.Features.Exam.Queries.GetExamDetail;
using Application.Features.Exam.Queries.GetExamList;
using Application.Features.Exams.Commands.CreateExam;
using Application.Features.Exams.Commands.RestoreExam;
using Application.Features.Exams.Commands.SoftDeleteExam;
using Application.Features.Exams.Queries.GetAdminExamList;
using AdminGetExamDetailQuery = Application.Features.Exams.Queries.GetExamDetail.GetExamDetailQuery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

public class ExamsController : ApiControllerBase
{
    /// <summary>
    /// Lấy danh sách đề thi (Có phân trang, tìm kiếm và lọc) - UC-3.1
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetList([FromQuery] GetExamListQuery query)
    {
        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách tất cả đề thi quản trị (bao gồm cả đề đã ẩn)
    /// </summary>
    [HttpGet("admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAdminList()
    {
        var result = await Mediator.Send(new GetAdminExamListQuery());

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một đề thi kèm lịch sử làm bài - UC-3.2
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail([FromRoute] int id, [FromQuery] int? userId)
    {
        var result = await Mediator.Send(new GetExamDetailQuery(id, userId));

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết cấu trúc đề thi, danh sách câu hỏi và lượt thi (quản trị)
    /// </summary>
    [HttpGet("admin/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdminDetail(int id)
    {
        var result = await Mediator.Send(new AdminGetExamDetailQuery { Id = id });

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

        return CreatedAtAction(nameof(GetAdminDetail), new { id = result.Data }, result);
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
