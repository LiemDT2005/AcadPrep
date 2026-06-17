using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Performance.Query.GetStudyStreak;

public record GetStudyStreakQuery(int UserId) : IRequest<Result<StudyStreakDto>>;

