using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Admin.Accounts.DTOs;
using AcadPrep.Application.Features.Admin.Accounts.Queries.GetAccountDetail;
using AcadPrep.Application.Features.Admin.Accounts.Commands.UpdateAccountStatus;
using AcadPrep.Application.Features.Admin.Accounts.Commands.AssignRole;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.WebUI.Pages.Admin.Accounts;

[Authorize(Roles = nameof(UserRole.Admin))]
public class DetailModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    public DetailModel(IMediator mediator, IAppDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    public AccountDetailDto? Account { get; set; }
    public List<Role> AvailableRoles { get; set; } = new();
    public List<ExamAttemptDto> RecentAttempts { get; set; } = new();

    public class ExamAttemptDto
    {
        public string ExamTitle { get; set; } = null!;
        public int TotalScore { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _mediator.Send(new GetAccountDetailQuery(id));
        if (!result.IsSuccess || result.Data == null)
        {
            TempData["ErrorMessage"] = result.Error ?? "Account not found.";
            return RedirectToPage("./Index");
        }

        Account = result.Data;
        AvailableRoles = await _context.Roles.ToListAsync();

        // Load recent attempts
        RecentAttempts = await _context.ExamAttempts
            .Where(ea => ea.UserId == id && ea.IsSubmitted)
            .OrderByDescending(ea => ea.CompletedAt)
            .Take(5)
            .Select(ea => new ExamAttemptDto
            {
                ExamTitle = ea.Exam.Title,
                TotalScore = ea.TotalScore,
                CompletedAt = ea.CompletedAt
            })
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string newStatus)
    {
        int currentAdminId = 1;
        var result = await _mediator.Send(new UpdateAccountStatusCommand(id, newStatus, currentAdminId));
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = $"Account status successfully updated to {newStatus}!";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAssignRoleAsync(int id, int newRoleId)
    {
        int currentAdminId = 1;
        var result = await _mediator.Send(new AssignRoleCommand(id, newRoleId, currentAdminId));
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Role assigned successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToPage(new { id });
    }
}
