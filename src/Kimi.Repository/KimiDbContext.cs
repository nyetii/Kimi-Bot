using Kimi.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Kimi.Repository;

public class KimiDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Guild> Guilds { get; set; }
    public DbSet<GuildUser> GuildUsers { get; set; }
    public DbSet<DailyScore> DailyScores { get; set; }

    public KimiDbContext() { }

    public KimiDbContext(DbContextOptions<KimiDbContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={Environment.CurrentDirectory}/Kimi.db");
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(e => e.Guilds)
            .WithMany(e => e.Users)
            .UsingEntity<GuildUser>();
    }
}