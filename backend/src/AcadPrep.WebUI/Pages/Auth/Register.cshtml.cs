using Application.Common.Exceptions;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.Commands.ResendOtp;
using Application.Features.Auth.Commands.VerifyOtp;
using Application.Features.Auth.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Auth;

public class RegisterModel(IMediator mediator) : PageModel
{
    [BindProperty]
    public RegisterInputModel Input { get; set; } = new();

    [BindProperty]
    public string? OtpCode { get; set; }

    public string? ErrorMessage { get; set; }
    public bool ShowOtpPanel { get; set; }
    public string? RegisteredEmail { get; set; }
    public OtpVerificationStatus? OtpStatus { get; set; }
    public int RemainingAttempts { get; set; }
    public bool ResendSuccess { get; set; }
    public bool ResendLocked { get; set; }
    public bool ResendExpired { get; set; }

    public void OnGet()
    {
        var email = TempData["RegisteredEmail"]?.ToString();
        var showOtp = TempData["ShowOtpPanel"]?.ToString();

        if (!string.IsNullOrEmpty(email) && showOtp == "true")
        {
            RegisteredEmail = email;
            ShowOtpPanel = true;
            TempData["RegisteredEmail"] = email;
            TempData["ShowOtpPanel"] = "true";
        }
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        try
        {
            var command = new RegisterCommand(
                Input.Email,
                Input.Password,
                Input.ConfirmPassword,
                Input.FullName
            );

            var result = await mediator.Send(command);

            TempData["RegisteredEmail"] = result.Email;
            TempData["ShowOtpPanel"] = "true";

            return RedirectToPage();
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError($"Input.{error.PropertyName}", error.ErrorMessage);
            }
            return Page();
        }
        catch (EmailAlreadyExistsException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostVerifyAsync()
    {
        var email = TempData["RegisteredEmail"]?.ToString();
        if (string.IsNullOrEmpty(email))
        {
            ErrorMessage = "Session expired. Please register again.";
            return Page();
        }

        TempData["RegisteredEmail"] = email;
        TempData["ShowOtpPanel"] = "true";

        try
        {
            var command = new VerifyOtpCommand(email, OtpCode ?? string.Empty);
            var result = await mediator.Send(command);

            OtpStatus = result.Status;
            RemainingAttempts = result.RemainingAttempts;
            RegisteredEmail = email;
            ShowOtpPanel = result.Status != OtpVerificationStatus.Success;

            if (result.Status == OtpVerificationStatus.Success)
            {
                return RedirectToPage("/Auth/Login", new { registered = "true" });
            }

            if (result.Status == OtpVerificationStatus.OtpExpired)
            {
                ErrorMessage = "Your verification code has expired. Please register again.";
                ShowOtpPanel = false;
                TempData.Remove("ShowOtpPanel");
                TempData.Remove("RegisteredEmail");
            }

            return Page();
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            ShowOtpPanel = true;
            RegisteredEmail = email;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostResendAsync()
    {
        var email = TempData["RegisteredEmail"]?.ToString();
        if (string.IsNullOrEmpty(email))
        {
            ErrorMessage = "Session expired. Please register again.";
            return Page();
        }

        TempData["RegisteredEmail"] = email;
        TempData["ShowOtpPanel"] = "true";
        RegisteredEmail = email;
        ShowOtpPanel = true;

        try
        {
            var command = new ResendOtpCommand(email);
            var result = await mediator.Send(command);

            ResendSuccess = result.IsSuccess;
            ResendLocked = result.IsLocked;
            ResendExpired = result.IsExpired;

            if (result.IsExpired)
            {
                ErrorMessage = "Your registration session has expired. Please register again.";
                ShowOtpPanel = false;
                TempData.Remove("ShowOtpPanel");
                TempData.Remove("RegisteredEmail");
            }

            return Page();
        }
        catch (ValidationException)
        {
            ErrorMessage = "Invalid email. Please register again.";
            ShowOtpPanel = false;
            return Page();
        }
    }
}

public class RegisterInputModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
