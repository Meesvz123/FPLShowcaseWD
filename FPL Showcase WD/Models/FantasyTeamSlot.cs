using System.ComponentModel.DataAnnotations;

namespace FPL_Showcase_WD.Models;

public sealed class FantasyTeamSlot
{
    public int Id { get; set; }

    public int FantasyTeamId { get; set; }
    public FantasyTeam? FantasyTeam { get; set; }

    public int PlayerId { get; set; }
    public Player? Player { get; set; }

    [Required, MaxLength(8)]
    public string Position { get; set; } = string.Empty;

    [Required, MaxLength(8)]
    public string Area { get; set; } = string.Empty;

    [Range(0, 14)]
    public int SlotIndex { get; set; }
}