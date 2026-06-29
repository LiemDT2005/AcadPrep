using AcadPrep.Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace AcadPrep.Application.Features.Practice.Commands.StartPractice;

public record StartPracticeCommand(
    int ExamId,
    List<int> SelectedPartNumbers,
    List<string> SelectedTags,
    int? TimeLimitMinutes,
    int UserId
) : IRequest<Result<int>>;
