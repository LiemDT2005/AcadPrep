using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Billing.Commands.GrantProSubscription;

public record GrantProSubscriptionCommand(
    int UserId,
    int PlanId,
    string? Note,
    int AdminUserId
) : IRequest<Result<GrantProSubscriptionResultDto>>;

public sealed class GrantProSubscriptionResultDto
{
    public int SubscriptionId { get; init; }
    public DateTime ExpiresAt { get; init; }
}
