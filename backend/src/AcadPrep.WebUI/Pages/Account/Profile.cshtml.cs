using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AcadPrep.Application.Features.Performance.Queries.GetExamAttempts;
using Application.Common.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AcadPrep.WebUI.Pages.Account;

[Authorize]
public class ProfileModel : PageModel
{
    private const long MaxAvatarBytes = 2 * 1024 * 1024;
    private static readonly HashSet<string> AllowedAvatarContentTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationService _notificationService;
    private readonly IMediator _mediator;

    public ProfileModel(
        IAppDbContext context,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher,
        IServiceProvider serviceProvider,
        INotificationService notificationService,
        IMediator mediator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
        _serviceProvider = serviceProvider;
        _notificationService = notificationService;
        _mediator = mediator;
    }

    public UserProfileViewModel Profile { get; private set; } = new();
    public bool IsStaff { get; private set; }
    public bool CanChangePassword { get; private set; }
    public string ActiveTab { get; private set; } = "overview";
    public ExamAttemptsResultDto ExamAttempts { get; private set; } = new();

    [BindProperty]
    public EditProfileInput ProfileInput { get; set; } = new();

    [BindProperty]
    public ChangePasswordInput PasswordInput { get; set; } = new();

    [BindProperty]
    public IFormFile? AvatarFile { get; set; }

    public async Task<IActionResult> OnGetAsync(string? tab = null)
    {
        ActiveTab = NormalizeTab(tab);
        return await LoadPageAsync() ? Page() : Unauthorized();
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        ActiveTab = "edit";
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized();
        }

        var fullName = ProfileInput.FullName?.Trim() ?? string.Empty;
        if (fullName.Length is < 2 or > 150)
        {
            ModelState.AddModelError("ProfileInput.FullName", "Display name must be between 2 and 150 characters.");
        }

        if (AvatarFile is not null)
        {
            if (AvatarFile.Length == 0 || AvatarFile.Length > MaxAvatarBytes)
            {
                ModelState.AddModelError("AvatarFile", "Choose a JPG, PNG, or WebP image up to 2 MB.");
            }
            else if (!AllowedAvatarContentTypes.Contains(AvatarFile.ContentType))
            {
                ModelState.AddModelError("AvatarFile", "Only JPG, PNG, and WebP images are supported.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulatePageAsync(user);
            return Page();
        }

        user.UpdateFullName(fullName);

        if (AvatarFile is not null)
        {
            var storage = _serviceProvider.GetRequiredService<IFileStorageService>();
            await using var stream = AvatarFile.OpenReadStream();
            var upload = await storage.UploadImageAsync(
                stream,
                $"{user.Id}-{Guid.NewGuid():N}{Path.GetExtension(AvatarFile.FileName)}",
                "acadprep/avatars");
            user.UpdateAvatar(upload.Url);
        }

        await _context.SaveChangesAsync();
        await RefreshAuthenticationCookieAsync(user);

        TempData["ProfileSuccess"] = "Profile updated successfully.";
        return RedirectToPage(new { tab = "edit" });
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        ActiveTab = "password";
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "This account uses Google sign-in and does not have a password to change.");
        }
        else if (string.IsNullOrWhiteSpace(PasswordInput.CurrentPassword)
            || !_passwordHasher.Verify(user.PasswordHash, PasswordInput.CurrentPassword))
        {
            ModelState.AddModelError("PasswordInput.CurrentPassword", "The current password is incorrect.");
        }

        if (!IsStrongPassword(PasswordInput.NewPassword))
        {
            ModelState.AddModelError(
                "PasswordInput.NewPassword",
                "Use at least 8 characters with uppercase, lowercase, number, and special character.");
        }

        if (!string.Equals(PasswordInput.NewPassword, PasswordInput.ConfirmPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError("PasswordInput.ConfirmPassword", "The password confirmation does not match.");
        }

        if (!ModelState.IsValid)
        {
            await PopulatePageAsync(user);
            return Page();
        }

        user.ChangePassword(_passwordHasher.Hash(PasswordInput.NewPassword!));
        await _context.SaveChangesAsync();

        // Thông báo bảo mật cho chủ tài khoản (UC-15)
        await _notificationService.CreateAsync(
            userId: user.Id,
            title: "Password changed successfully",
            message: "Your account password has been changed successfully. If this was not you, please secure your account immediately.",
            type: NotificationType.SecurityPasswordChanged,
            linkUrl: "/Account/Profile?tab=password");

        TempData["ProfileSuccess"] = "Password changed successfully.";
        return RedirectToPage(new { tab = "password" });
    }

    private async Task<bool> LoadPageAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return false;
        }

        await PopulatePageAsync(user);
        return true;
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        if (!int.TryParse(_currentUserService.UserId, out var userId))
        {
            return null;
        }

        return await _context.Users
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Id == userId);
    }

    private async Task PopulatePageAsync(User user)
    {
        IsStaff = user.Role.RoleName is nameof(UserRole.Admin) or nameof(UserRole.Moderator);
        CanChangePassword = !string.IsNullOrWhiteSpace(user.PasswordHash);

        Profile = new UserProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role.RoleName,
            Status = user.Status.ToString(),
            CreatedAt = user.CreatedAt
        };

        if (IsStaff)
        {
            Profile.Metrics =
            [
                new("Published exams", await _context.Exams.CountAsync(exam => exam.Status == ExamStatus.Published), "assignment"),
                new("Question bank", await _context.Questions.CountAsync(), "quiz"),
                new("Achievements", await _context.Achievements.CountAsync(), "military_tech"),
                new("Active learners", await _context.Users.CountAsync(item =>
                    item.Status == UserStatus.Active && item.Role.RoleName == nameof(UserRole.Learner)), "groups")
            ];
        }
        else
        {
            var completedAttempts = _context.ExamAttempts
                .Where(attempt => attempt.UserId == user.Id && attempt.IsSubmitted);

            Profile.Metrics =
            [
                new("Best TOEIC score", await completedAttempts.MaxAsync(attempt => (int?)attempt.TotalScore) ?? 0, "workspace_premium"),
                new("Completed exams", await completedAttempts.CountAsync(), "description"),
                new("Study streak", await _context.StudyStreaks
                    .Where(streak => streak.UserId == user.Id)
                    .Select(streak => (int?)streak.CurrentStreak)
                    .FirstOrDefaultAsync() ?? 0, "local_fire_department", "days"),
                new("Saved vocabulary", await _context.SavedVocabularies.CountAsync(item => item.UserId == user.Id), "menu_book")
            ];
        }

        ProfileInput.FullName = user.FullName;

        var attemptsResult = await _mediator.Send(new GetExamAttemptsQuery(user.Id));
        ExamAttempts = attemptsResult.Data ?? new ExamAttemptsResultDto();
    }

    private async Task RefreshAuthenticationCookieAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.RoleName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }

    private static bool IsStrongPassword(string? password)
    {
        return password is { Length: >= 8 }
            && password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(character => !char.IsLetterOrDigit(character));
    }

    private static string NormalizeTab(string? tab)
    {
        return tab is "edit" or "password" or "attempts" ? tab : "overview";
    }
}

public class EditProfileInput
{
    [Display(Name = "Display name")]
    public string? FullName { get; set; }
}

public class ChangePasswordInput
{
    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }

    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }
}

public class UserProfileViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<ProfileMetricViewModel> Metrics { get; set; } = [];
}

public record ProfileMetricViewModel(string Label, int Value, string Icon, string? Suffix = null);
