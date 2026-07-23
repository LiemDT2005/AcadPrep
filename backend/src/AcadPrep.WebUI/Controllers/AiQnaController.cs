using System.Threading.Tasks;
using AcadPrep.Application.Features.AiQna.Commands.AskAi;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcadPrep.WebUI.Controllers;

[Authorize(Policy = "RequireLearnerRole")]
[ApiController]
[Route("api/ai-qna")]
public class AiQnaController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiQnaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskAiCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(new { error = result.Error });
    }
}
