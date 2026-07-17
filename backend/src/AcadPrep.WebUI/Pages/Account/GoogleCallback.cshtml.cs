using AcadPrep.Application.Common.Models;
using Application.Features.Auth.Commands.GoogleCallback;
using Application.Features.Auth.Commands.Login;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace AcadPrep.WebUI.Pages.Account;

/// <summary>
/// PageModel xử lý Google OAuth callback (UC-5.2).
/// Nhận AuthenticateResult từ Google, build GoogleCallbackCommand, gọi Mediator.
/// Nếu thành công → SignInAsync cookie + redirect.
/// </summary>
public class GoogleCallbackModel(ISender mediator) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        // Lấy kết quả xác thực từ Google OAuth middleware
        var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!authenticateResult.Succeeded)
        {
            // Xác thực Google thất bại hoặc user hủy → redirect về trang Login kèm thông báo
            return RedirectToPage("/Account/Login",
                new { error = "Đăng nhập bằng Google thất bại. Vui lòng thử lại." });
        }

        var principal = authenticateResult.Principal;

        // Lấy claims từ Google ID Token
        var email    = principal.FindFirstValue(ClaimTypes.Email)    ?? string.Empty;
        var googleId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var fullName = principal.FindFirstValue(ClaimTypes.Name)     ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(googleId))
        {
            return RedirectToPage("/Account/Login",
                new { error = "Không thể lấy thông tin từ tài khoản Google." });
        }

        var command = new GoogleCallbackCommand(email, googleId, fullName);
        var result  = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            // Tài khoản bị khóa (Suspended)
            return RedirectToPage("/Account/Login", new { error = result.Error });
        }

        var dto = result.Data!;

        // Phát cookie xác thực — concern của Presentation layer
        await SignInUserAsync(dto);

        return LocalRedirect(GetPostLoginDestination(returnUrl, dto.Role));
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

    private string GetPostLoginDestination(string? returnUrl, string role)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl)
            && Url.IsLocalUrl(returnUrl)
            && returnUrl is not ("/" or "/Index"))
        {
            return returnUrl;
        }

        return role switch
        {
            nameof(UserRole.Admin) => "/Admin/Dashboard",
            nameof(UserRole.Moderator) => "/Admin/Exams",
            _ => "/Performance/Dashboard"
        };
    }
}
