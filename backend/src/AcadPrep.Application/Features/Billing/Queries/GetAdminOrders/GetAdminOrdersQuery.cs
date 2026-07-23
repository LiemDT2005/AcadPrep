using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Billing.Queries.GetAdminOrders;

public record GetAdminOrdersQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? Status = null,
    string? Search = null
) : IRequest<Result<PaginatedList<AdminOrderDto>>>;

public sealed class AdminOrderDto
{
    public int Id { get; init; }
    public string OrderCode { get; init; } = null!;
    public int UserId { get; init; }
    public string UserEmail { get; init; } = null!;
    public string UserFullName { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public long AmountVnd { get; init; }
    public string Status { get; init; } = null!;
    public string PaymentProvider { get; init; } = null!;
    public string? ProviderTxnId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PaidAt { get; init; }
}
