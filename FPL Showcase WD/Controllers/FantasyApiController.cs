using FPL_Showcase_WD.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FPL_Showcase_WD.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/fantasy")]
public sealed class FantasyApiController(AppDbContext db) : ControllerBase
{
    [HttpGet("players")]
    public async Task<IActionResult> GetPlayers()
    {
        var players = await db.Players.AsNoTracking().ToListAsync();
        return Ok(players);
    }
}

