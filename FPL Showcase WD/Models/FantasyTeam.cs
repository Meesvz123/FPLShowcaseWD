using System.ComponentModel.DataAnnotations;

namespace FPL_Showcase_WD.Models;

public sealed class FantasyTeam
{
    public int Id { get; set; }

    [Required, MaxLength(60)]
    public string Name { get; set; } = "Mijn team";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<FantasyTeamSlot> Slots { get; set; } = [];
}