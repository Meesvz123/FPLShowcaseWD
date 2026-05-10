using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPL_Showcase_WD.Models;

public sealed class FantasyTeam
{
    public int Id { get; set; }
    [NotMapped]
    public Guid TeamIdentifier { get; set; } = Guid.NewGuid();

    [Required, MaxLength(450)]
    public string ApplicationUserId { get; set; } = string.Empty;

    public ApplicationUser? ApplicationUser { get; set; }

    [Required, MaxLength(60)]
    public string Name { get; set; } = "Mijn team";
        
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<FantasyTeamSlot> Slots { get; set; } = [];
}