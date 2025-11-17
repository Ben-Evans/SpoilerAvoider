namespace SpoilerFreeHighlights.Core.Persistence;

using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public static string DbPath { get; } = Path.Combine(AppContext.BaseDirectory, "Resources");
    public static string DbPathWithFile { get; } = Path.Combine(AppContext.BaseDirectory, "Resources", "spoiler-avoider.db");
    public static string DbFile { get; } = "spoiler-avoider.db";

    public DbSet<League> Leagues { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<YouTubePlaylist> YouTubePlaylists { get; set; }
    public DbSet<YouTubeVideo> YouTubeVideos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>()
            .HasKey(x => new { x.LeagueId, x.Id });

        modelBuilder.Entity<Game>()
            .HasKey(x => new { x.LeagueId, x.Id });

        modelBuilder.Entity<Game>()
            .HasOne(x => x.HomeTeam)
            .WithMany()
            .HasForeignKey(x => new { x.LeagueId, x.HomeTeamId })
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Game>()
            .HasOne(x => x.AwayTeam)
            .WithMany()
            .HasForeignKey(x => new { x.LeagueId, x.AwayTeamId })
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        base.OnConfiguring(optionsBuilder);
    }
}
