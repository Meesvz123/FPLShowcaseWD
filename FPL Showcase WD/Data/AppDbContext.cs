    using FPL_Showcase_WD.Models;
using Microsoft.EntityFrameworkCore;

namespace FPL_Showcase_WD.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
}