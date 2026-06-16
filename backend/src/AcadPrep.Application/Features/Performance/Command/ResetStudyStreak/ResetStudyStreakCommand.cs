using MediatR;

namespace AcadPrep.Application.Features.Performance.Command.ResetStudyStreak;

public record ResetStudyStreakCommand(int UserId) : IRequest;
