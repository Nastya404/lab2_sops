using Microsoft.EntityFrameworkCore;

namespace Critters.Shared.Data;

public class CrittersContext : DbContext
{
    public CrittersContext(DbContextOptions<CrittersContext> options) : base(options) { }

    public DbSet<Player> Roster => Set<Player>();
}
