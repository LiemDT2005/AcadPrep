using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.AiQna.Commands.AskAi;

namespace Application.Common.Interfaces;

public interface IAiChatService
{
    Task<Result<AiChatResponse>> AskAsync(string systemPrompt, IReadOnlyList<ChatMessageDto> history, string newMessage, CancellationToken cancellationToken = default);
}

public record AiChatResponse(string Text, int TokensUsed, string ModelUsed);
