using FPL_Showcase_WD.Models;
using Microsoft.EntityFrameworkCore;

namespace FPL_Showcase_WD.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<FantasyTeam> FantasyTeams => Set<FantasyTeam>();
    public DbSet<FantasyTeamSlot> FantasyTeamSlots => Set<FantasyTeamSlot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FantasyTeamSlot>()
            .HasOne(s => s.FantasyTeam)
            .WithMany(t => t.Slots)
            .HasForeignKey(s => s.FantasyTeamId);

        modelBuilder.Entity<FantasyTeamSlot>()
            .HasOne(s => s.Player)
            .WithMany()
            .HasForeignKey(s => s.PlayerId);
    }
}