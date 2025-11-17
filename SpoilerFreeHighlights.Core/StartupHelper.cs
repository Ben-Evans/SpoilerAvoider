using Microsoft.Extensions.DependencyInjection;

namespace SpoilerFreeHighlights.Core;

public static class StartupHelper
{
    public static void SetupDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var databaseProvider = configuration["DatabaseProvider"];

        if (databaseProvider == "SqlServer")
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));
        }
        else
        {
            services.AddDbContext<AppDbContext>(options => options
                .UseSqlite($"Data Source={AppDbContext.DbPathWithFile}")
                .EnableSensitiveDataLogging());
        }
    }

    public static async Task SeedDatabase(this IServiceProvider serviceProvider)
    {
        // Seed DB if it doesn't exist already.
        Directory.CreateDirectory(AppDbContext.DbPath);
        using IServiceScope scope = serviceProvider.CreateScope();

        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated(); // .Migrate; -- If adding migrations clear database.

        bool leaguesSeeded = dbContext.Leagues.Any();
        if (!leaguesSeeded)
        {
            Log.Logger.Information($"Seeding {nameof(Leagues)} into the database...");
            dbContext.Leagues.AddRange(
                [
                    new League { Id = Leagues.Nhl, Name = Leagues.Nhl.Name },
                    new League { Id = Leagues.Mlb, Name = Leagues.Mlb.Name },
                    new League { Id = Leagues.Cfl, Name = Leagues.Cfl.Name }
                ]);
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
        services.AddScoped<YouTubeRssService>();
    }
}
