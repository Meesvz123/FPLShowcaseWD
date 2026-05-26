using System.ComponentModel.DataAnnotations;

namespace FPL_Showcase_WD.Models;

public sealed class EditFantasyTeamViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    public string? UserEmail { get; set; }

    public List<SlotEditItem> Slots { get; set; } = new();

    public List<PlayerOption> Players { get; set; } = new();
}

public sealed class SlotEditItem
{
    public int SlotIndex { get; set; }

    public string Position { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;

    public int? PlayerId { get; set; }

    public string Label { get; set; } = string.Empty;
}

public sealed class PlayerOption
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Club { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public int Price { get; set; }
}