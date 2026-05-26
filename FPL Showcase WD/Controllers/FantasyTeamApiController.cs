using FPL_Showcase_WD.Data;
using FPL_Showcase_WD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FPL_Showcase_WD.Controllers;

[ApiController]
[Authorize]
[Route("api/fantasy/team")]
public sealed class FantasyTeamApiController(AppDbContext db, UserManager<ApplicationUser> userManager) : ControllerBase
{
    public sealed record SwapRequest(int PlayerOutId, int PlayerInId);

    [HttpGet]
    public async Task<ActionResult<FantasyTeamDto>> GetMyTeam([FromQuery] int? teamId)
    {
        FantasyTeam? team;

        if (teamId.HasValue)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            team = await db.FantasyTeams
                .Include(t => t.Slots)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == teamId.Value);
        }
        else
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            team = await db.FantasyTeams
                .Include(t => t.Slots)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ApplicationUserId == userId);
        }

        if (team is null)
        {
            return NotFound();
        }

        return Ok(FantasyTeamDto.From(team));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTeam([FromBody] SaveFantasyTeamRequest request, [FromQuery] int? teamId)
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

        FantasyTeam? team;

        if (teamId.HasValue)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            team = await db.FantasyTeams
                .Include(t => t.Slots)
                .FirstOrDefaultAsync(t => t.Id == teamId.Value);

            if (team is null)
            {
                return NotFound();
            }
        }
        else
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            team = await db.FantasyTeams
                .Include(t => t.Slots)
                .FirstOrDefaultAsync(t => t.ApplicationUserId == userId);

            if (team is null)
            {
                team = new FantasyTeam
                {
                    ApplicationUserId = userId,
                    Name = request.Name.Trim()
                };
                db.FantasyTeams.Add(team);
            }
        }

        team.Name = request.Name.Trim();
        db.FantasyTeamSlots.RemoveRange(team.Slots);
        team.Slots.Clear();

        team.Slots = request.Slots.Select(s => new FantasyTeamSlot
        {
            PlayerId = s.PlayerId,
            Position = s.Position,
            Area = s.Area,
            SlotIndex = s.SlotIndex
        }).ToList();

        await db.SaveChangesAsync();

        return Ok(new { team.TeamIdentifier, team.Name });
    }

    [HttpPatch("wissel/{teamIdentifier:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Wissel(Guid teamIdentifier, [FromBody] SwapRequest request)
    {
        var team = await db.FantasyTeams
            .Include(t => t.Slots)
            .FirstOrDefaultAsync(t => t.TeamIdentifier == teamIdentifier);

        if (team is null)
        {
            return NotFound();
        }

        var userId = userManager.GetUserId(User);
        if (team.ApplicationUserId != userId)
        {
            return Forbid();
        }

        var slotToSwap = team.Slots.FirstOrDefault(s => s.PlayerId == request.PlayerOutId);
        if (slotToSwap is null)
        {
            return BadRequest("Wisselspeler is geen onderdeel van het team.");
        }

        var incomingPlayerExists = await db.Players.AnyAsync(p => p.Id == request.PlayerInId);
        if (!incomingPlayerExists)
        {
            return BadRequest("Inkomende speler bestaat niet.");
        }

        slotToSwap.PlayerId = request.PlayerInId;
        await db.SaveChangesAsync();

        return Ok(FantasyTeamDto.From(team));
    }
}