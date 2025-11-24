namespace SpoilerFreeHighlights.Core.Persistence;

using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public static string DbPath { get; } = Path.Combine(AppContext.BaseDirectory, "Resources");

    public DbSet<League> Leagues { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Game> Games { get; set; }

    public DbSet<YouTubePlaylist> YouTubePlaylists { get; set; }
    public DbSet<YouTubeVideo> YouTubeVideos { get; set; }

    public DbSet<LeagueConfiguration> LeagueConfigurations { get; set; }
    public DbSet<PlaylistConfiguration> PlaylistConfigurations { get; set; }
    public DbSet<VideoTitleIdentifier> VideoTitleIdentifiers { get; set; }
    public DbSet<VideoTitleTeamFormat> VideoTitleTeamFormats { get; set; }
    public DbSet<VideoTitleDateFormat> VideoTitleDateFormats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<League>()
            .Property(x => x.Id)
            .ValueGeneratedNever();

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

        modelBuilder.Entity<LeagueConfiguration>()
            .HasKey(x => x.LeagueId);

        modelBuilder.Entity<LeagueConfiguration>()
            .HasOne(x => x.League)
            .WithMany()
            .HasForeignKey(x => x.LeagueId);

        modelBuilder.Entity<LeagueConfiguration>()
            .HasMany(x => x.Playlists)
            .WithOne()
            .HasForeignKey(x => x.LeagueConfigurationId);

        modelBuilder.Entity<LeagueConfiguration>()
            .Navigation(x => x.Playlists)
            .AutoInclude();

        modelBuilder.Entity<PlaylistConfiguration>()
            .HasMany(x => x.TitleIdentifiers)
            .WithOne()
            .HasForeignKey(x => x.PlaylistConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlaylistConfiguration>()
            .HasMany(x => x.TeamFormats)
            .WithOne()
            .HasForeignKey(x => x.PlaylistConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlaylistConfiguration>()
            .HasMany(x => x.DateFormats)
            .WithOne()
            .HasForeignKey(x => x.PlaylistConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlaylistConfiguration>()
            .Navigation(x => x.TitleIdentifiers)
            .AutoInclude();

        modelBuilder.Entity<PlaylistConfiguration>()
            .Navigation(x => x.TeamFormats)
            .AutoInclude();

        modelBuilder.Entity<PlaylistConfiguration>()
            .Navigation(x => x.DateFormats)
            .AutoInclude();

        modelBuilder.Entity<PlaylistConfiguration>()
            .Property(x => x.TitlePattern)
            .HasMaxLength(1024);

        modelBuilder.Entity<PlaylistConfiguration>()
            .Property(x => x.Comment)
            .HasMaxLength(2048);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        base.OnConfiguring(optionsBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>().HaveMaxLength(256);

        base.ConfigureConventions(configurationBuilder);
    }
}
