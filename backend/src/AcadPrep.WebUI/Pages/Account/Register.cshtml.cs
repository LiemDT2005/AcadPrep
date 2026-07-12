using AcadPrep.Application.Common.Models;
using Application.Features.Auth.Commands.Register;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Account;

/// <summary>
/// PageModel cho trang Register (UC-1 — Đăng ký tài khoản bằng Email).
/// KHÔNG chứa bất kỳ business logic nào — tất cả ủy quyền cho MediatR.
/// Sau khi đăng ký thành công, redirect sang /Account/VerifyOtp để nhập OTP (UC-2).
/// </summary>
public class RegisterModel(ISender mediator) : PageModel
{
    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    [BindProperty]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Thông báo lỗi hiển thị trên form (null = không hiển thị).</summary>
    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet()
    {
        // Nếu đã đăng nhập → redirect về Home
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    /// <summary>
    /// UC-1: Xử lý form POST đăng ký.
    /// Kết quả: fail (validation/business) / success (redirect sang VerifyOtp).
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        var command = new RegisterCommand(Email, Password, ConfirmPassword, FullName);

        Result<RegisterResultDto> result;
        try
        {
            result = await mediator.Send(command);
        }
        catch (ValidationException ex)
        {
            // ValidationBehavior ném ValidationException khi field sai format/rỗng
            ErrorMessage = string.Join(" ", ex.Errors.Select(e => e.ErrorMessage));
            return Page();
        }

        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error;
            return Page();
        }

        // Đăng ký thành công — OTP đã gửi về email, redirect sang trang nhập OTP
        return RedirectToPage("/Account/VerifyOtp", new { email = Email, isReactivation = false });
    }
}
