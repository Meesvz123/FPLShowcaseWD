namespace FPL_Showcase_WD.Models;

public sealed record FantasyTeamDto(Guid Id, string Name, List<FantasyTeamSlotDto> Slots)
{
    public static FantasyTeamDto From(FantasyTeam team)
        => new(
            team.TeamIdentifier,
            team.Name,
            team.Slots
                .OrderBy(s => s.SlotIndex)
                .Select(s => new FantasyTeamSlotDto(s.PlayerId, s.Position, s.Area, s.SlotIndex))
                .ToList());
}

public sealed record FantasyTeamSlotDto(int PlayerId, string Position, string Area, int SlotIndex);