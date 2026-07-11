using System.ComponentModel.DataAnnotations;
using AcadPrep.Application.Common.Models;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Achievements.Commands.EditAchievement;

public class EditAchievementCommand : IRequest<Result<int>>
{
    public int AchievementId { get; set; }

    [Required(ErrorMessage = "Badge name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    public string? IconUrl { get; set; }

    [Required(ErrorMessage = "Condition type is required")]
    public string ConditionType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Condition value is required")]
    public int ConditionValue { get; set; }
}
