using FPL_Showcase_WD.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FPL_Showcase_WD.Controllers;

[Authorize]
public sealed class FantasyTeamController(AppDbContext db) : Controller
{
    public IActionResult Index() => View();

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> All()
    {
        var teams = await db.FantasyTeams
            .Include(t => t.ApplicationUser)
            .Include(t => t.Slots)
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync();

        return View(teams);
    }
}