using AcadPrep.Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace AcadPrep.Application.Features.AiQna.Commands.AskAi;

public record AskAiCommand(string Message, IReadOnlyList<ChatMessageDto> History) : IRequest<Result<AskAiResultDto>>;
