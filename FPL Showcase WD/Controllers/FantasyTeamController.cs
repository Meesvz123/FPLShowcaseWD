using FPL_Showcase_WD.Data;
using FPL_Showcase_WD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FPL_Showcase_WD.Controllers;

[Authorize]
public sealed class FantasyTeamController(AppDbContext db) : Controller
{
    private static readonly SlotDefinition[] SlotDefinitions =
    [
        new(0, "FWD", "field", "Aanvaller 1"),
        new(1, "FWD", "field", "Aanvaller 2"),
        new(2, "FWD", "field", "Aanvaller 3"),
        new(3, "MID", "field", "Middenvelder 1"),
        new(4, "MID", "field", "Middenvelder 2"),
        new(5, "MID", "field", "Middenvelder 3"),
        new(6, "DEF", "field", "Verdediger 1"),
        new(7, "DEF", "field", "Verdediger 2"),
        new(8, "DEF", "field", "Verdediger 3"),
        new(9, "DEF", "field", "Verdediger 4"),
        new(10, "GK", "field", "Keeper"),
        new(11, "GK", "bench", "Reserve keeper"),
        new(12, "DEF", "bench", "Reserve verdediger"),
        new(13, "MID", "bench", "Reserve middenvelder"),
        new(14, "FWD", "bench", "Reserve aanvaller")
    ];

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

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var team = await db.FantasyTeams
            .Include(t => t.ApplicationUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team is null)
        {
            return NotFound();
        }

        var model = await BuildEditModelAsync(team);
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditFantasyTeamViewModel model)
    {
        var team = await db.FantasyTeams
            .Include(t => t.ApplicationUser)
            .FirstOrDefaultAsync(t => t.Id == model.Id);

        if (team is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulatePlayersAsync(model);
            ApplySlotDefinitions(model);
            model.UserEmail = team.ApplicationUser?.Email;
            return View(model);
        }

        var slotMap = SlotDefinitions.ToDictionary(s => s.SlotIndex);
        var selectedPlayers = new HashSet<int>();
        var newSlots = new List<FantasyTeamSlot>();

        foreach (var slot in model.Slots)
        {
            if (!slot.PlayerId.HasValue)
            {
                continue;
            }

            if (!slotMap.TryGetValue(slot.SlotIndex, out var definition))
            {
                continue;
            }

            if (!selectedPlayers.Add(slot.PlayerId.Value))
            {
                ModelState.AddModelError(string.Empty, "Een speler mag maar één keer gekozen worden.");
                await PopulatePlayersAsync(model);
                ApplySlotDefinitions(model);
                model.UserEmail = team.ApplicationUser?.Email;
                return View(model);
            }

            newSlots.Add(new FantasyTeamSlot
            {
                FantasyTeamId = team.Id,
                PlayerId = slot.PlayerId.Value,
                SlotIndex = slot.SlotIndex,
                Position = definition.Position,
                Area = definition.Area
            });
        }

        team.Name = model.Name.Trim();

        var existingSlots = await db.FantasyTeamSlots
            .Where(s => s.FantasyTeamId == team.Id)
            .ToListAsync();

        db.FantasyTeamSlots.RemoveRange(existingSlots);
        db.FantasyTeamSlots.AddRange(newSlots);

        await db.SaveChangesAsync();

        return RedirectToAction(nameof(All));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var team = await db.FantasyTeams
            .Include(t => t.Slots)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team is not null)
        {
            db.FantasyTeamSlots.RemoveRange(team.Slots);
            db.FantasyTeams.Remove(team);
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(All));
    }

    private async Task<EditFantasyTeamViewModel> BuildEditModelAsync(FantasyTeam team)
    {
        var slots = await db.FantasyTeamSlots
            .Where(s => s.FantasyTeamId == team.Id)
            .AsNoTracking()
            .ToListAsync();

        var players = await db.Players
            .AsNoTracking()
            .OrderBy(p => p.Positie)
            .ThenBy(p => p.Naam)
            .ToListAsync();

        var model = new EditFantasyTeamViewModel
        {
            Id = team.Id,
            Name = team.Name,
            UserEmail = team.ApplicationUser?.Email,
            Players = players.Select(p => new PlayerOption
            {
                Id = p.Id,
                Name = p.Naam,
                Club = p.Club,
                Position = p.Positie,
                Price = p.Prijs
            }).ToList(),
            Slots = SlotDefinitions.Select(def => new SlotEditItem
            {
                SlotIndex = def.SlotIndex,
                Position = def.Position,
                Area = def.Area,
                Label = def.Label,
                PlayerId = slots.FirstOrDefault(s => s.SlotIndex == def.SlotIndex)?.PlayerId
            }).ToList()
        };

        return model;
    }

    private async Task PopulatePlayersAsync(EditFantasyTeamViewModel model)
    {
        var players = await db.Players
            .AsNoTracking()
            .OrderBy(p => p.Positie)
            .ThenBy(p => p.Naam)
            .ToListAsync();

        model.Players = players.Select(p => new PlayerOption
        {
            Id = p.Id,
            Name = p.Naam,
            Club = p.Club,
            Position = p.Positie,
            Price = p.Prijs
        }).ToList();
    }

    private static void ApplySlotDefinitions(EditFantasyTeamViewModel model)
    {
        var slotLookup = model.Slots.ToDictionary(s => s.SlotIndex);
        model.Slots = SlotDefinitions.Select(def =>
        {
            slotLookup.TryGetValue(def.SlotIndex, out var existing);
            return new SlotEditItem
            {
                SlotIndex = def.SlotIndex,
                Position = def.Position,
                Area = def.Area,
                Label = def.Label,
                PlayerId = existing?.PlayerId
            };
        }).ToList();
    }

    private sealed record SlotDefinition(int SlotIndex, string Position, string Area, string Label);
}