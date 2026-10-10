using Microsoft.EntityFrameworkCore;
using CrittersApp.Models;

namespace CrittersApp.Data
{
    public class CrittersContext : DbContext
    {
        public CrittersContext(DbContextOptions<CrittersContext> options) : base(options) { }

        public DbSet<Roster> Roster { get; set; } = null!;
    }
}