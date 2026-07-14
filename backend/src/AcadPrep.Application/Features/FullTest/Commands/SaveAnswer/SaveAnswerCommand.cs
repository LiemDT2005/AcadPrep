using AcadPrep.Application.Common.Models;
using Domain.Enums;
using MediatR;

namespace AcadPrep.Application.Features.FullTest.Commands.SaveAnswer;

public record SaveAnswerCommand(int AttemptId, int UserId, int QuestionId, string? SelectedOption) : IRequest<Result>;
