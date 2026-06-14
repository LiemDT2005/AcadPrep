using AcadPrep.Application.Features.Performance.DTOs;
using MediatR;

namespace AcadPrep.Application.Features.Performance.Queries.GetStudyStreak;

public record GetStudyStreakQuery(int UserId) : IRequest<StudyStreakDto>;
