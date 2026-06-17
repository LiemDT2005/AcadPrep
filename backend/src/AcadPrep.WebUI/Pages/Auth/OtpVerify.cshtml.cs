using Application.Features.Auth.Commands.ResendOtp;
using Application.Features.Auth.Commands.VerifyOtp;
using Application.Features.Auth.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Auth;

public class OtpVerifyModel(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Email { get; set; }

    [BindProperty]
    public string? OtpCode { get; set; }

    public string? ErrorMessage { get; set; }
    public OtpVerificationStatus? OtpStatus { get; set; }
    public int RemainingAttempts { get; set; }
    public bool ResendSuccess { get; set; }
    public bool ResendLocked { get; set; }

    public IActionResult OnGet()
    {
        if (string.IsNullOrEmpty(Email))
        {
            return RedirectToPage("/Auth/Register");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostVerifyAsync()
    {
        if (string.IsNullOrEmpty(Email))
        {
            ErrorMessage = "Email is required.";
            return Page();
        }

        try
        {
            var command = new VerifyOtpCommand(Email, OtpCode ?? string.Empty);
            var result = await mediator.Send(command);

            OtpStatus = result.Status;
            RemainingAttempts = result.RemainingAttempts;

            if (result.Status == OtpVerificationStatus.Success)
            {
                return RedirectToPage("/Auth/Login", new { registered = "true" });
            }

            if (result.Status == OtpVerificationStatus.OtpExpired)
            {
                ErrorMessage = "Your verification code has expired. Please register again.";
                return RedirectToPage("/Auth/Register");
            }

            return Page();
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return Page();
        }
    }

    public async Task<IActionResult> OnPostResendAsync(string email)
    {
        Email = email;

        if (string.IsNullOrEmpty(email))
        {
            ErrorMessage = "Email is required.";
            return Page();
        }

        try
        {
            var command = new ResendOtpCommand(email);
            var result = await mediator.Send(command);

            ResendSuccess = result.IsSuccess;
            ResendLocked = result.IsLocked;

            if (result.IsExpired)
            {
                ErrorMessage = "Your registration session has expired. Please register again.";
                return RedirectToPage("/Auth/Register");
            }

            return Page();
        }
        catch (ValidationException)
        {
            ErrorMessage = "Invalid email. Please register again.";
            return RedirectToPage("/Auth/Register");
        }
    }
}
