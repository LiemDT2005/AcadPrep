using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Admin.Accounts.DTOs;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Accounts.Queries.GetAccountDetail;

public record GetAccountDetailQuery(int UserId) : IRequest<Result<AccountDetailDto>>;
