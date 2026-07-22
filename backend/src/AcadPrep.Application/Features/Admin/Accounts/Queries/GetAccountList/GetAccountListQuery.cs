using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Admin.Accounts.DTOs;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Accounts.Queries.GetAccountList;

public record GetAccountListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    string? RoleFilter = null,
    string? StatusFilter = null
) : IRequest<Result<PaginatedList<AccountListItemDto>>>;
