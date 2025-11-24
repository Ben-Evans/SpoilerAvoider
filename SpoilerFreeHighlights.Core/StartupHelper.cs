using Microsoft.Extensions.DependencyInjection;

namespace SpoilerFreeHighlights.Core;

public static class StartupHelper
{
    public static void SetupDatabase(this IServiceCollection services, IConfiguration configuration, bool useSqlite)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection")?.Replace("%APP_DB_DIR%", AppDbContext.DbPath);
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Database connection string is not configured.");

        if (useSqlite)
        {
            services.AddDbContext<AppDbContext>(options => options
                .UseSqlite(connectionString)
                .EnableSensitiveDataLogging());
        }
        else
        {
            services.AddDbContext<AppDbContext>(options => options
                .UseSqlServer(connectionString)
                .EnableSensitiveDataLogging());
        }
    }

    public static async Task ApplyMigrationsAndSeedDatabase(this IServiceProvider serviceProvider, bool useSqlite)
    {
        if (useSqlite)
            Directory.CreateDirectory(AppDbContext.DbPath);

        using IServiceScope scope = serviceProvider.CreateScope();

        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
            await dbContext.Database.MigrateAsync();

        bool leaguesSeeded = await dbContext.Leagues.AnyAsync();
        if (!leaguesSeeded)
        {
            Log.Logger.Information($"Seeding {nameof(Leagues)} into the database...");
            dbContext.Leagues.AddRange(Leagues.GetAllLeagues().Select(x => new League { Id = x, Name = x.Name }));
            dbContext.SaveChanges();
            Log.Logger.Information($"{nameof(Leagues)} seeded successfully.");
        }

        using HttpClient httpClient = new();

        bool nhlTeamsSeeded = dbContext.Teams.Any(x => x.LeagueId == Leagues.Nhl);
        if (!nhlTeamsSeeded)
            await NhlService.SeedTeams(dbContext, httpClient);

        bool mlbTeamsSeeded = dbContext.Teams.Any(x => x.LeagueId == Leagues.Mlb);
        if (!mlbTeamsSeeded)
            await MlbService.SeedTeams(dbContext, httpClient);

        bool cflTeamsSeeded = dbContext.Teams.Any(x => x.LeagueId == Leagues.Cfl);
        if (!cflTeamsSeeded)
            await CflService.SeedTeams(dbContext);
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<NhlService>();
        services.AddScoped<MlbService>();
        services.AddScoped<CflService>();
        services.AddScoped<LeaguesService>();
        //services.AddSingleton<YouTubeService>();
        services.AddScoped<YouTubeService>();
    }
}
