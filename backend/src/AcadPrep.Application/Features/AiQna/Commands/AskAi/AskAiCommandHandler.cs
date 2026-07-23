using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Common.Constants;
using Application.Common.Interfaces;
using MediatR;
using System.Text.RegularExpressions;

namespace AcadPrep.Application.Features.AiQna.Commands.AskAi;

internal sealed class AskAiCommandHandler : IRequestHandler<AskAiCommand, Result<AskAiResultDto>>
{
    private readonly IAiChatService _aiChatService;
    private readonly IQuotaService _quotaService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBillingAccessService _billingService;

    public AskAiCommandHandler(
        IAiChatService aiChatService,
        IQuotaService quotaService,
        ICurrentUserService currentUserService,
        IBillingAccessService billingService)
    {
        _aiChatService = aiChatService;
        _quotaService = quotaService;
        _currentUserService = currentUserService;
        _billingService = billingService;
    }

    public async Task<Result<AskAiResultDto>> Handle(AskAiCommand request, CancellationToken cancellationToken)
    {
        if (!int.TryParse(_currentUserService.UserId, out int userId))
        {
            return Result<AskAiResultDto>.Failure("User not found.");
        }

        bool isPro = await _billingService.IsProAsync(userId, cancellationToken);

        // a. Ước lượng token
        int historyLength = request.History?.Sum(x => x.Content.Length) ?? 0;
        int estimatedTokens = (request.Message.Length + historyLength) / 4;
        if (estimatedTokens < 1) estimatedTokens = 1;

        // b. Kiểm tra và trừ quota
        var quotaResult = await _quotaService.CheckAndConsumeAsync(userId, isPro, estimatedTokens, cancellationToken);
        if (!quotaResult.IsSuccess)
        {
            return Result<AskAiResultDto>.Failure(quotaResult.Error ?? "Vượt quá giới hạn token hôm nay.");
        }
        
        int remainingTokens = quotaResult.Data;

        // c. Gọi AI Service
        var aiResult = await _aiChatService.AskAsync(
            SystemPromptTemplates.AiQna,
            request.History ?? Array.Empty<ChatMessageDto>(),
            request.Message,
            cancellationToken);

        if (!aiResult.IsSuccess || aiResult.Data == null)
        {
            // Có thể rollback token ở đây nếu cần, nhưng đơn giản thì cứ bỏ qua (coi như vẫn trừ token do API request cost)
            return Result<AskAiResultDto>.Failure(aiResult.Error ?? "AI đang bận, thử lại sau.");
        }

        string reply = aiResult.Data.Text;

        // d. AnswerLeakGuard
        if (IsAnswerLeak(reply))
        {
            reply = "Rất tiếc, mình không thể đưa ra đáp án trực tiếp cho bài tập của bạn. Tuy nhiên, nếu bạn muốn hỏi về ngữ pháp hay từ vựng trong câu này, mình sẵn sàng giải thích.";
        }

        return Result<AskAiResultDto>.Success(new AskAiResultDto(reply, remainingTokens));
    }

    private bool IsAnswerLeak(string reply)
    {
        // Simple regex heuristic to prevent leaks if the AI disobeys
        var lower = reply.ToLower();
        
        // Kiểm tra các cụm từ tiếng Việt tiết lộ đáp án
        if (lower.Contains("đáp án đúng là") || 
            lower.Contains("chọn đáp án") || 
            Regex.IsMatch(lower, @"đáp án (chính xác )?(là )?[abcd]\b"))
        {
            return true;
        }

        return false;
    }
}
