using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Billing.Commands.CreatePaymentOrder;

public record CreatePaymentOrderCommand(
    int UserId,
    int PlanId,
    string ClientIp,
    string AbsoluteReturnUrl
) : IRequest<Result<CreatePaymentOrderResultDto>>;

public sealed class CreatePaymentOrderResultDto
{
    public int OrderId { get; init; }
    public string OrderCode { get; init; } = null!;
    public string PaymentUrl { get; init; } = null!;
}
