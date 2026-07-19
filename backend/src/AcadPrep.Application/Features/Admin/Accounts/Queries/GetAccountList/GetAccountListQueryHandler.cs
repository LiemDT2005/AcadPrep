using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Admin.Accounts.DTOs;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Features.Admin.Accounts.Queries.GetAccountList;

public class GetAccountListQueryHandler : IRequestHandler<GetAccountListQuery, Result<PaginatedList<AccountListItemDto>>>
{
    private readonly IAppDbContext _context;

    public GetAccountListQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<AccountListItemDto>>> Handle(
        GetAccountListQuery request, CancellationToken cancellationToken)
    {
        // The first Admin user seeded (Id = 1) is the Master Admin
        var masterAdminId = await _context.Users
            .Where(u => u.Role.RoleName == nameof(UserRole.Admin))
            .OrderBy(u => u.Id)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var query = _context.Users
            .Include(u => u.Role)
            .AsQueryable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search));
        }

        // Role filter
        if (!string.IsNullOrWhiteSpace(request.RoleFilter) && request.RoleFilter != "all")
        {
            query = query.Where(u => u.Role.RoleName == request.RoleFilter);
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(request.StatusFilter) && request.StatusFilter != "all")
        {
            if (Enum.TryParse<UserStatus>(request.StatusFilter, true, out var status))
            {
                query = query.Where(u => u.Status == status);
            }
        }

        // Order: Admins first, then Moderators, then Learners; within each role by name
        query = query.OrderBy(u => u.Role.RoleName == nameof(UserRole.Admin) ? 0 :
                                     u.Role.RoleName == nameof(UserRole.Moderator) ? 1 : 2)
                     .ThenBy(u => u.FullName);

        var projected = query.Select(u => new AccountListItemDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            AvatarUrl = u.AvatarUrl,
            RoleName = u.Role.RoleName,
            Status = u.Status.ToString(),
            CreatedAt = u.CreatedAt,
            IsMasterAdmin = u.Id == masterAdminId
        });

        var result = await PaginatedList<AccountListItemDto>.CreateAsync(
            projected, request.PageNumber, request.PageSize);

        return Result<PaginatedList<AccountListItemDto>>.Success(result);
    }
}
