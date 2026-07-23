using System.Security.Claims;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.WebUI.ViewComponents;

public class ProfileAvatarViewComponent : ViewComponent
{
    private readonly IAppDbContext _context;
    private readonly IBillingAccessService _billingAccessService;

    public ProfileAvatarViewComponent(IAppDbContext context, IBillingAccessService billingAccessService)
    {
        _context = context;
        _billingAccessService = billingAccessService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string variant)
    {
        var userIdValue = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return Content(string.Empty);
        }

        var user = await _context.Users
            .AsNoTracking()
            .Include(item => item.Role)
            .FirstOrDefaultAsync(item => item.Id == userId);

        if (user is null)
        {
            return Content(string.Empty);
        }
        
        bool isPro = await _billingAccessService.IsProAsync(userId);

        return View(new ProfileAvatarViewModel
        {
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role.RoleName,
            Variant = variant,
            IsPro = isPro
        });
    }
}

public class ProfileAvatarViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Variant { get; set; } = string.Empty;
    public bool IsPro { get; set; }
}
