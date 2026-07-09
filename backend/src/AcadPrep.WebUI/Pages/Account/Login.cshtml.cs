using AcadPrep.Application.Common.Models;
using Application.Features.Auth.Commands.Login;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace AcadPrep.WebUI.Pages.Account;

/// <summary>
/// PageModel cho trang Login (UC-5.1 — Login with Email).
/// Cũng expose OnGetGoogleLoginAsync để khởi động OAuth flow (UC-5.2).
/// KHÔNG chứa bất kỳ business logic nào — tất cả ủy quyền cho MediatR.
/// </summary>
public class LoginModel(ISender mediator) : PageModel
{
    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    /// <summary>Thông báo lỗi hiển thị trên form (null = không hiển thị).</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>Hiển thị khi tài khoản cần xác minh email (Status == Inactive).</summary>
    public bool RequiresVerification { get; private set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        // Nếu đã đăng nhập → redirect về Home
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return Page();
    }

    /// <summary>
    /// UC-5.1: Xử lý form POST login bằng email + password.
    /// Kết quả: fail / requires-verification / success (SignInAsync + redirect).
    /// </summary>
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        var command = new LoginCommand(Email, Password);

        Result<LoginResultDto> result;
        try
        {
            result = await mediator.Send(command);
        }
        catch (ValidationException ex)
        {
            // ValidationBehavior ném ValidationException khi field rỗng/sai format
            ErrorMessage = string.Join(" ", ex.Errors.Select(e => e.ErrorMessage));
            return Page();
        }

        // Nhánh 1: Handler trả Failure (sai mật khẩu, bị khóa...)
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error;
            return Page();
        }

        var dto = result.Data!;

        // Nhánh 2: Tài khoản cần xác minh email (Inactive)
        if (dto.RequiresVerification)
        {
            RequiresVerification = true;
            return Page();
        }

        // Nhánh 3: Đăng nhập thành công → dựng ClaimsPrincipal và phát cookie
        await SignInUserAsync(dto);

        return LocalRedirect(returnUrl ?? "/");
    }

    /// <summary>
    /// UC-5.2 — Bước khởi động: Challenge Google OAuth.
    /// Trình duyệt sẽ redirect sang Google login page.
    /// </summary>
    public IActionResult OnGetGoogleLogin(string? returnUrl = null)
    {
        var redirectUrl = Url.Page("/Account/GoogleCallback", pageHandler: null,
            values: new { returnUrl });

        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private async Task SignInUserAsync(LoginResultDto dto)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, dto.UserId.ToString()),
            new(ClaimTypes.Email,          dto.Email),
            new(ClaimTypes.Name,           dto.FullName),
            new(ClaimTypes.Role,           dto.Role)
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });
    }
}
