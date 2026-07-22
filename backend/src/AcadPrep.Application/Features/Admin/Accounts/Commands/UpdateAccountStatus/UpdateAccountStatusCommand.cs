using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Accounts.Commands.UpdateAccountStatus;

public record UpdateAccountStatusCommand(
    int UserId,
    string NewStatus,
    int CurrentAdminId
) : IRequest<Result>;
