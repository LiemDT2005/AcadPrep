using System.Linq;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AchievementsController : ControllerBase
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AchievementsController(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    [HttpGet("unnotified")]
    public async Task<IActionResult> GetUnnotifiedAchievements()
    {
        var userIdStr = _currentUserService.UserId;
        // Since we don't have proper Auth set up in the code base based on my research,
        // Let's fallback to User ID 2 (Test User) for demonstration purposes if auth is missing
        int userId = 2; 
        if (int.TryParse(userIdStr, out int parsedId))
        {
            userId = parsedId;
        }

        var unnotified = await _context.UserAchievements
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId && !ua.IsNotified)
            .Select(ua => new
            {
                ua.AchievementId,
                ua.Achievement.Name,
                ua.Achievement.Description,
                ua.Achievement.IconUrl
            })
            .ToListAsync();

        return Ok(unnotified);
    }

    [HttpPost("mark-notified")]
    public async Task<IActionResult> MarkAsNotified([FromBody] int achievementId)
    {
        var userIdStr = _currentUserService.UserId;
        int userId = 2; // Fallback
        if (int.TryParse(userIdStr, out int parsedId))
        {
            userId = parsedId;
        }

        var userAchievement = await _context.UserAchievements
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

        if (userAchievement != null)
        {
            userAchievement.IsNotified = true;
            await _context.SaveChangesAsync(default);
            return Ok();
        }

        return NotFound();
    }
}
