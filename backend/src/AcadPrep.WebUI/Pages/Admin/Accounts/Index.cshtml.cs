using System.Collections.Generic;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Admin.Accounts.DTOs;
using AcadPrep.Application.Features.Admin.Accounts.Queries.GetAccountList;
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
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    public IndexModel(IMediator mediator, IAppDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    public PaginatedList<AccountListItemDto>? Accounts { get; set; }
    public List<Role> AvailableRoles { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RoleFilter { get; set; } = "all";

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; } = "all";

    public async Task<IActionResult> OnGetAsync()
    {
        // Load accounts
        var result = await _mediator.Send(new GetAccountListQuery(
            PageNumber,
            10,
            Search,
            RoleFilter,
            StatusFilter
        ));

        if (result.IsSuccess)
        {
            Accounts = result.Data;
        }

        // Load roles for assign role dropdown
        AvailableRoles = await _context.Roles.ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int userId, string newStatus)
    {
        // Get current admin user ID (assuming ClaimTypes.NameIdentifier or equivalent is set)
        int currentAdminId = 1; // Default fallback or fetch from User identity

        var result = await _mediator.Send(new UpdateAccountStatusCommand(userId, newStatus, currentAdminId));
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = $"Account status updated successfully to {newStatus}!";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToPage("./Index", new { pageNumber = PageNumber, search = Search, roleFilter = RoleFilter, statusFilter = StatusFilter });
    }

    public async Task<IActionResult> OnPostAssignRoleAsync(int userId, int newRoleId)
    {
        int currentAdminId = 1; // Default fallback or fetch from User identity

        var result = await _mediator.Send(new AssignRoleCommand(userId, newRoleId, currentAdminId));
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Account role updated successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToPage("./Index", new { pageNumber = PageNumber, search = Search, roleFilter = RoleFilter, statusFilter = StatusFilter });
    }
}
