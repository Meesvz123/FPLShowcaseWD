    using FPL_Showcase_WD.Data;
using FPL_Showcase_WD.Models;
using FPL_Showcase_WD.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FPL_Showcase_WD.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/sync")]
public sealed class PlayerSyncController(
    AppDbContext db,
    IApiFootballClient apiFootballClient,
    IOptions<PlayerSyncOptions> options) : ControllerBase
{
    [HttpPost("players")]
    public async Task<IActionResult> SyncPlayers([FromQuery] bool force = false)
    {
        if (!force && await db.Players.AnyAsync())
        {
            var count = await db.Players.CountAsync();
            return Ok(new { count, skipped = true });
        }

        var season = options.Value.Season;
        var apiPlayers = await apiFootballClient.GetPremierLeaguePlayersAsync(season);
        if (apiPlayers.Count == 0)
        {
            return NotFound("No players returned by API.");
        }

        await db.Database.ExecuteSqlRawAsync("DELETE FROM `Players`;");

        var entities = apiPlayers
            .GroupBy(p => p.Player.Id)
            .Select(g => g.First())
            .Select(p =>
            {
                var stats = p.Statistics.FirstOrDefault();
                var position = MapPosition(stats?.Games?.Position);
                var club = stats?.Team?.Name ?? string.Empty;

                return new Player
                {
                    Id = p.Player.Id,
                    Naam = p.Player.Name,
                    Positie = position,
                    Club = club,
                    Prijs = 0,
                    Statistieken = 0
                };
            }).ToList();

        db.Players.AddRange(entities);
        await db.SaveChangesAsync();

        return Ok(new { count = entities.Count });
    }

    private static string MapPosition(string? apiPosition)
    {
        return apiPosition?.Trim().ToLowerInvariant() switch
        {
            "goalkeeper" => "GK",
            "defender" => "DEF",
            "midfielder" => "MID",
            "attacker" => "FWD",
            "forward" => "FWD",
            _ => "BENCH"
        };
    }
}