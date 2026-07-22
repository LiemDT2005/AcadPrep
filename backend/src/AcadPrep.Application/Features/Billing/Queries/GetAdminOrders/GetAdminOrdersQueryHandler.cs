using AcadPrep.Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Billing.Queries.GetAdminOrders;

public sealed class GetAdminOrdersQueryHandler
    : IRequestHandler<GetAdminOrdersQuery, Result<PaginatedList<AdminOrderDto>>>
{
    private readonly IAppDbContext _context;

    public GetAdminOrdersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<AdminOrderDto>>> Handle(
        GetAdminOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Plan)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(o => o.Status.ToString() == request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(o =>
                o.OrderCode.Contains(s) ||
                o.User.Email.Contains(s) ||
                o.User.FullName.Contains(s) ||
                (o.ProviderTxnId != null && o.ProviderTxnId.Contains(s)));
        }

        var projected = query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new AdminOrderDto
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                UserId = o.UserId,
                UserEmail = o.User.Email,
                UserFullName = o.User.FullName,
                PlanName = o.Plan.Name,
                AmountVnd = o.AmountVnd,
                Status = o.Status.ToString(),
                PaymentProvider = o.PaymentProvider.ToString(),
                ProviderTxnId = o.ProviderTxnId,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt
            });

        var page = await PaginatedList<AdminOrderDto>.CreateAsync(
            projected,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<AdminOrderDto>>.Success(page);
    }
}
