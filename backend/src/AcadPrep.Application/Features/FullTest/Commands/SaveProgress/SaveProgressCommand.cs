using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.FullTest.Commands.SaveProgress;

public record SaveProgressCommand(int AttemptId, int UserId, int RemainingSeconds) : IRequest<Result>;
