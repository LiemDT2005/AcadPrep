using AcadPrep.Application.Features.Admin.DTOs;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Queries.GetUserStats;

public record GetUserStatsQuery() : IRequest<UserStatsDto>;

public class GetUserStatsQueryHandler : IRequestHandler<GetUserStatsQuery, UserStatsDto>
{
    public Task<UserStatsDto> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new UserStatsDto
        {
            TotalUsers = 0,
            ActiveUsers = 0,
            NewUsersThisMonth = 0
        });
    }
}
