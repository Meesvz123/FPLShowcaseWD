using FPL_Showcase_WD.Data;
using FPL_Showcase_WD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FPL_Showcase_WD.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/fantasy/team")]
public sealed class FantasyTeamApiController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTeam([FromBody] SaveFantasyTeamRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (request.Slots.Count is < 1 or > 15)
        {
            return BadRequest("Team must contain 1-15 players.");
        }

        if (request.Slots.Select(s => s.PlayerId).Distinct().Count() != request.Slots.Count)
        {
            return BadRequest("Duplicate players are not allowed.");
        }

        if (request.Slots.Select(s => s.SlotIndex).Distinct().Count() != request.Slots.Count)
        {
            return BadRequest("Duplicate slots are not allowed.");
        }

        var playerIds = request.Slots.Select(s => s.PlayerId).Distinct().ToList();
        var existing = await db.Players.CountAsync(p => playerIds.Contains(p.Id));
        if (existing != playerIds.Count)
        {
            return BadRequest("One or more players do not exist.");
        }

        var team = new FantasyTeam
        {
            Name = request.Name.Trim()
        };

        team.Slots = request.Slots.Select(s => new FantasyTeamSlot
        {
            PlayerId = s.PlayerId,
            Position = s.Position,
            Area = s.Area,
            SlotIndex = s.SlotIndex
        }).ToList();

        db.FantasyTeams.Add(team);
        await db.SaveChangesAsync();

        return Ok(new { team.Id, team.Name });
    }
}