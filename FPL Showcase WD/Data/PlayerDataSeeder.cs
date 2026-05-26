using FPL_Showcase_WD.Models;
using FPL_Showcase_WD.Services;
using Microsoft.EntityFrameworkCore;

namespace FPL_Showcase_WD.Data;

public sealed class PlayerDataSeeder(
    AppDbContext db,
    IApiFootballClient apiClient,
    ILogger<PlayerDataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var season = GetCurrentSeason();
        var apiPlayers = await apiClient.GetPremierLeaguePlayersAsync(season);

        if (apiPlayers.Count == 0)
        {
            await FixExistingPlayersAsync(cancellationToken);
            return;
        }

        var existingPlayers = await db.Players.ToListAsync(cancellationToken);
        if (existingPlayers.Count == 0)
        {
            var players = apiPlayers
                .Select(MapFromApi)
                .Where(p => !string.IsNullOrWhiteSpace(p.Naam))
                .GroupBy(p => NormalizeKey(p.Naam, p.Club))
                .Select(g => g.First())
                .ToList();

            db.Players.AddRange(players);
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        var apiLookup = apiPlayers
            .Select(p => new
            {
                Key = NormalizeKey(p.Player.Name, p.Statistics.FirstOrDefault()?.Team.Name ?? string.Empty),
                Position = NormalizePosition(p.Statistics.FirstOrDefault()?.Games.Position)
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .GroupBy(x => x.Key)
            .ToDictionary(g => g.Key, g => g.First().Position);

        var updated = false;
        foreach (var player in existingPlayers)
        {
            var key = NormalizeKey(player.Naam, player.Club);
            if (!string.IsNullOrWhiteSpace(key) &&
                apiLookup.TryGetValue(key, out var position) &&
                string.IsNullOrWhiteSpace(player.Positie))
            {
                player.Positie = position;
                updated = true;
            }

            if (player.Prijs <= 0)
            {
                player.Prijs = CalculatePrice(player);
                updated = true;
            }

            if (string.IsNullOrWhiteSpace(player.Positie))
            {
                player.Positie = "MID";
                updated = true;
            }
        }

        if (updated)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static Player MapFromApi(PlayerResponse response)
    {
        var club = response.Statistics.FirstOrDefault()?.Team.Name ?? "Onbekend";
        var position = NormalizePosition(response.Statistics.FirstOrDefault()?.Games.Position);

        var player = new Player
        {
            Naam = response.Player.Name,
            Club = club,
            Positie = position,
            Statistieken = 0
        };

        player.Prijs = CalculatePrice(player);
        return player;
    }

    private static int CalculatePrice(Player player)
    {
        const int min = 4_000_000;
        const int max = 10_000_000;

        var score = player.Statistieken > 0
            ? Math.Clamp(player.Statistieken, 0, 100)
            : Math.Abs(HashCode.Combine(player.Naam, player.Club)) % 101;

        return min + (int)Math.Round((max - min) * (score / 100.0));
    }

    private static string NormalizePosition(string? position)
        => position?.Trim().ToLowerInvariant() switch
        {
            "goalkeeper" => "GK",
            "defender" => "DEF",
            "midfielder" => "MID",
            "attacker" => "FWD",
            "forward" => "FWD",
            _ => "MID"
        };

    private static string NormalizeKey(string name, string club)
        => $"{name}|{club}".Trim().ToLowerInvariant();

    private static int GetCurrentSeason()
        => DateTime.UtcNow.Month < 7 ? DateTime.UtcNow.Year - 1 : DateTime.UtcNow.Year;

    private async Task FixExistingPlayersAsync(CancellationToken cancellationToken)
    {
        var players = await db.Players.ToListAsync(cancellationToken);
        if (players.Count == 0)
        {
            return;
        }

        var updated = false;
        foreach (var player in players)
        {
            if (player.Prijs <= 0)
            {
                player.Prijs = CalculatePrice(player);
                updated = true;
            }

            if (string.IsNullOrWhiteSpace(player.Positie))
            {
                player.Positie = "MID";
                updated = true;
            }
        }

        if (updated)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}