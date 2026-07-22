using MediatR;

namespace AcadPrep.Application.Features.Performance.Commands.ResetStudyStreak;

public record ResetStudyStreakCommand(int UserId) : IRequest;
