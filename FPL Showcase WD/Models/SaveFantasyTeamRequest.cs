using System.ComponentModel.DataAnnotations;

namespace FPL_Showcase_WD.Models;

public sealed class SaveFantasyTeamRequest
{
    [Required, MaxLength(60)]
    public string Name { get; set; } = "Mijn team";

    [Required, MinLength(1), MaxLength(15)]
    public List<SaveFantasyTeamSlotDto> Slots { get; set; } = [];
}

public sealed class SaveFantasyTeamSlotDto
{
    [Range(1, int.MaxValue)]
    public int PlayerId { get; set; }

    [Required, RegularExpression("^(GK|DEF|MID|FWD)$")]
    public string Position { get; set; } = string.Empty;

    [Required, RegularExpression("^(field|bench)$")]
    public string Area { get; set; } = string.Empty;

    [Range(0, 14)]
    public int SlotIndex { get; set; }
}