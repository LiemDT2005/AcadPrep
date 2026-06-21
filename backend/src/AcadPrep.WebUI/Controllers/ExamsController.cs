using System.Threading.Tasks;
using Application.Features.Exam.Queries.GetExamList;
using Application.Features.Exam.Queries.GetExamDetail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WebUI.Controllers;

public class ExamsController : ApiControllerBase
{
    /// <summary>
    /// Lấy danh sách đề thi (Có phân trang, tìm kiếm và lọc) - UC-3.1
    /// </summary>
    /// <param name="query">Tham số tìm kiếm, lọc theo bộ đề, năm và phân trang</param>
    /// <returns>Danh sách đề thi đã phân trang</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/Exams?Search=ets&amp;SeriesName=ETS TOEIC&amp;Year=2024&amp;PageIndex=1&amp;PageSize=6
    ///
    /// </remarks>
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
    /// Lấy thông tin chi tiết một đề thi kèm lịch sử làm bài - UC-3.2
    /// </summary>
    /// <param name="id">ID của đề thi</param>
    /// <param name="userId">ID của người học (tùy chọn, dùng để hiển thị lịch sử làm bài)</param>
    /// <returns>Chi tiết đề thi và danh sách lượt làm bài trước đó</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/Exams/1?userId=2
    ///
    /// </remarks>
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
}
