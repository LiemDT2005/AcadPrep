using AcadPrep.Application.Common.Models;
using Application.Features.Auth.Commands.ForgotPassword;
using Application.Features.Auth.Commands.ResetPassword;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Account;

/// <summary>
/// PageModel cho trang ForgotPassword (UC-8 — Quên mật khẩu).
/// Multi-step: Bước 1 (nhập email) → Bước 2 (nhập OTP + mật khẩu mới).
/// KHÔNG chứa bất kỳ business logic nào — tất cả ủy quyền cho MediatR.
/// Email được giữ qua TempData để redirect POST → GET an toàn (PRG pattern).
/// KHÔNG lưu OTP hay password ở phía client.
/// </summary>
public class ForgotPasswordModel(ISender mediator) : PageModel
{
    // ── Bước 1: Nhập email ───────────────────────────────────────────────────
    [BindProperty]
    public string Email { get; set; } = string.Empty;

    // ── Bước 2: Nhập OTP + mật khẩu mới ────────────────────────────────────
    [BindProperty]
    public string OtpCode { get; set; } = string.Empty;

    [BindProperty]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Bước hiện tại: 1 = nhập email, 2 = nhập OTP + mật khẩu mới.
    /// Được điều khiển bởi logic server — không phụ thuộc client input.
    /// </summary>
    public int Step { get; private set; } = 1;

    /// <summary>Thông báo thành công hiển thị trên form (null = không hiển thị).</summary>
    public string? SuccessMessage { get; private set; }

    /// <summary>Thông báo lỗi hiển thị trên form (null = không hiển thị).</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>Hiển thị Step 1 — form nhập email.</summary>
    public IActionResult OnGet()
    {
        // Nếu đã đăng nhập → redirect về Home
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        Step = 1;
        return Page();
    }

    /// <summary>
    /// UC-8 Bước 1: Gửi OTP reset password về email.
    /// Anti-enumeration: response giống nhau bất kể email có tồn tại hay không.
    /// Sau khi gửi, chuyển sang Step 2 (nhập OTP + mật khẩu mới).
    /// </summary>
    public async Task<IActionResult> OnPostSendOtpAsync()
    {
        var command = new ForgotPasswordCommand(Email);

        ForgotPasswordResultDto? dto = null;
        try
        {
            var result = await mediator.Send(command);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error;
                Step = 1;
                return Page();
            }

            dto = result.Data;
        }
        catch (ValidationException ex)
        {
            ErrorMessage = string.Join(" ", ex.Errors.Select(e => e.ErrorMessage));
            Step = 1;
            return Page();
        }

        // Giữ email qua TempData để dùng ở Step 2
        TempData["ForgotPassword_Email"] = Email;
        TempData["ForgotPassword_Message"] = dto?.Message;

        // PRG: redirect sang GET để hiển thị Step 2 (tránh form resubmit)
        return RedirectToPage(new { step = 2 });
    }

    /// <summary>
    /// UC-8 Bước 2: Đặt lại mật khẩu sau khi xác minh OTP.
    /// Thành công → redirect sang /Account/Login với thông báo.
    /// </summary>
    public async Task<IActionResult> OnPostResetAsync()
    {
        var emailFromSession = TempData.Peek("ForgotPassword_Email") as string ?? Email;

        var command = new ResetPasswordCommand(
            emailFromSession,
            OtpCode,
            NewPassword,
            ConfirmPassword);

        try
        {
            var result = await mediator.Send(command);

            if (!result.IsSuccess)
            {
                // Giữ email + message để hiển thị lại Step 2
                TempData.Keep("ForgotPassword_Email");
                TempData.Keep("ForgotPassword_Message");
                ErrorMessage = result.Error;
                Step = 2;
                Email = emailFromSession;
                return Page();
            }
        }
        catch (ValidationException ex)
        {
            TempData.Keep("ForgotPassword_Email");
            TempData.Keep("ForgotPassword_Message");
            ErrorMessage = string.Join(" ", ex.Errors.Select(e => e.ErrorMessage));
            Step = 2;
            Email = emailFromSession;
            return Page();
        }

        // Đặt lại mật khẩu thành công → về trang Login
        TempData["Login_SuccessMessage"] = "Mật khẩu đã được đặt lại thành công. Vui lòng đăng nhập với mật khẩu mới.";
        return RedirectToPage("/Account/Login");
    }

    /// <summary>
    /// GET handler cho step=2 — hiển thị form nhập OTP + mật khẩu mới.
    /// Email được lấy từ TempData (được set ở OnPostSendOtpAsync).
    /// </summary>
    public IActionResult OnGetStep2()
    {
        var emailFromSession = TempData.Peek("ForgotPassword_Email") as string;

        if (string.IsNullOrWhiteSpace(emailFromSession))
        {
            // Không có session hợp lệ → về Step 1
            return RedirectToPage();
        }

        Email = emailFromSession;
        SuccessMessage = TempData.Peek("ForgotPassword_Message") as string;
        TempData.Keep("ForgotPassword_Email");
        TempData.Keep("ForgotPassword_Message");
        Step = 2;
        return Page();
    }
}
