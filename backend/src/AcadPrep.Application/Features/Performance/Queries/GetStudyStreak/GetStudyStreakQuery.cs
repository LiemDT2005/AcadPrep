using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Performance.Queries.GetStudyStreak;

public record GetStudyStreakQuery(int UserId) : IRequest<Result<StudyStreakDto>>;

