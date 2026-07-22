using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Accounts.Commands.AssignRole;

public record AssignRoleCommand(
    int UserId,
    int NewRoleId,
    int CurrentAdminId
) : IRequest<Result>;
