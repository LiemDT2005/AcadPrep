using Application.Features.Auth.Commands.ResendOtp;
using Application.Features.Auth.Commands.VerifyOtp;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Account;

/// <summary>
/// PageModel cho trang VerifyOtp (UC-2 — Xác minh OTP).
/// Dùng chung cho cả flow Đăng ký (isReactivation=false) và flow Tái kích hoạt
/// (isReactivation=true). isReactivation chỉ ảnh hưởng UI text — KHÔNG truyền
/// vào bất kỳ Command nào (Handler đọc từ OtpCacheEntry trong Redis).
/// </summary>
public class VerifyOtpModel(ISender mediator) : PageModel
{
    /// <summary>Email nhận OTP — hiển thị trên UI để user biết đang xác minh email nào.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// true = flow tái kích hoạt → tiêu đề "Kích hoạt lại tài khoản".
    /// false = flow đăng ký mới → tiêu đề "Xác thực tài khoản".
    /// Chỉ dùng cho hiển thị UI, không truyền vào Command.
    /// </summary>
    public bool IsReactivation { get; private set; }

    public IActionResult OnGet(string email, bool isReactivation = false)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage("/Account/Register");
        }

        Email          = email;
        IsReactivation = isReactivation;
        return Page();
    }

    /// <summary>
    /// UC-2 Verify: nhận JSON { Email, OtpCode }, trả JSON { success, error, redirectTo }.
    /// </summary>
    public async Task<IActionResult> OnPostVerifyAsync([FromBody] VerifyOtpRequestModel model)
    {
        var command = new VerifyOtpCommand(model.Email, model.OtpCode);

        try
        {
            var result = await mediator.Send(command);

            if (!result.IsSuccess)
            {
                return new JsonResult(new { success = false, error = result.Error });
            }

            return new JsonResult(new
            {
                success    = true,
                error      = (string?)null,
                redirectTo = "/Account/Login"
            });
        }
        catch (ValidationException ex)
        {
            var message = string.Join(" ", ex.Errors.Select(e => e.ErrorMessage));
            return new JsonResult(new { success = false, error = message });
        }
    }

    /// <summary>
    /// UC-2 Resend: nhận JSON { Email }, trả JSON { success, error }.
    /// </summary>
    public async Task<IActionResult> OnPostResendAsync([FromBody] ResendOtpRequestModel model)
    {
        var command = new ResendOtpCommand(model.Email);

        try
        {
            var result = await mediator.Send(command);

            return new JsonResult(new
            {
                success = result.IsSuccess,
                error   = result.IsSuccess ? null : result.Error
            });
        }
        catch (ValidationException ex)
        {
            var message = string.Join(" ", ex.Errors.Select(e => e.ErrorMessage));
            return new JsonResult(new { success = false, error = message });
        }
    }
}

/// <summary>Request body model cho OnPostVerifyAsync.</summary>
public sealed record VerifyOtpRequestModel(string Email, string OtpCode);

/// <summary>Request body model cho OnPostResendAsync.</summary>
public sealed record ResendOtpRequestModel(string Email);
