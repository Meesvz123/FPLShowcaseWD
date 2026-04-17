namespace FPL_Showcase_WD.Models;

public sealed class PlayerSyncOptions
{
    public int Season { get; set; } = 2024;
    public string Endpoint { get; set; } = "/api/sync/players";
}