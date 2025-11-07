using Microsoft.AspNetCore.Mvc;
using MudBlazor;
using MudBlazor.Services;
using SpoilerFreeHighlights.Server.Components;

var builder = WebApplication.CreateBuilder(args);

Environment.SetEnvironmentVariable("APP_BASE_DIR", AppContext.BaseDirectory, EnvironmentVariableTarget.Process);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
    //.AddInteractiveServerComponents();

builder.Host.UseSerilog();

builder.Services.Configure<YouTubeSettings>(
    builder.Configuration.GetSection(YouTubeRssConstants.YouTubeSectionName));

// Register HttpClient for Server and Client DI.
builder.Services.AddHttpClient("Default", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue("BaseAddress", "https://localhost:7137/"));
});
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Default"));

//DbContextOptionsBuilder.EnableSensitiveDataLogging;

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseSqlite($"Data Source={AppDbContext.DbPathWithFile}")
    .EnableSensitiveDataLogging());

builder.Services.AddScoped<NhlService>();
builder.Services.AddScoped<MlbService>();
builder.Services.AddScoped<CflService>();
builder.Services.AddScoped<LeaguesService>();
//builder.Services.AddSingleton<YouTubeService>();
builder.Services.AddScoped<YouTubeRssService>();
//builder.Services.AddHostedService<DataCleanupService>();
//builder.Services.AddHostedService<LeagueScheduleRefreshService>();
builder.Services.AddHostedService<YouTubeRssRefreshService>();

builder.Services.AddMudServices();

//MudGlobal.UnhandledExceptionHandler = (exception) => Log.Logger.Error(exception, "Globally Caught Exeption");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    //.AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(SpoilerFreeHighlights.Client._Imports).Assembly);


app.MapGet("/api/reset", async (AppDbContext dbContext) =>
{
    await dbContext.YouTubeVideos.ExecuteDeleteAsync();
    await dbContext.YouTubePlaylists.ExecuteDeleteAsync();
    await dbContext.Games.ExecuteDeleteAsync();
    //await dbContext.Teams.ExecuteDeleteAsync();
    //await dbContext.Leagues.ExecuteDeleteAsync();
    await dbContext.SaveChangesAsync();
});

app.MapGet("/api/test/s", async (LeaguesService leaguesService) =>
{
    await leaguesService.FetchAndCacheScheduledGames();
});

app.MapGet("/api/test/yt", async (YouTubeRssService youTubeRssService) =>
{
    await youTubeRssService.FetchAndCacheNewVideos();

    await youTubeRssService.AddYouTubeLinksToAllMatches();
});

app.MapGet("/api/GetTeams", async (AppDbContext dbContext) =>
{
    return await dbContext.Teams.ToArrayAsync();
});

app.MapPost("/api/GetGameDays", async (
    NhlService nhlService,
    MlbService mlbService,
    CflService cflService,
    [FromBody] ScheduleQuery scheduleQuery) =>
{
    Dictionary<Leagues, LeagueService> services = new()
    {
        { Leagues.Nhl, nhlService },
        { Leagues.Mlb, mlbService },
        { Leagues.Cfl, cflService }
    };

    if (scheduleQuery.Leagues is null || scheduleQuery.Leagues.Length == 0 || scheduleQuery.Leagues.Contains(Leagues.All))
        scheduleQuery.Leagues = services.Keys.ToArray();

    List<Schedule> schedules = new();
    foreach (Leagues league in scheduleQuery.Leagues)
    {
        LeagueService leagueService = services[league];

        Schedule? leagueSchedule = await leagueService.GetScheduleForThisWeek(scheduleQuery.UserPreferences);
        if (leagueSchedule is null)
            return Results.Problem("Failed to retrieve data from external API.", statusCode: 500);

        schedules.Add(leagueSchedule);
    }

    Schedule allSchedule = new()
    {
        League = Leagues.All,
        GameDays = schedules
            .SelectMany(x => x.GameDays)
            .GroupBy(x => x.DateLeague)
            .Select(x => new GameDay
            {
                DateLeague = x.Key,
                Games = x.SelectMany(y => y.Games).OrderBy(y => y.StartDateUtc).ToList()
            })
            .ToList()
    };

    return allSchedule is not null
        ? Results.Ok(allSchedule)
        : Results.Problem("Failed to deserialize external API response.", statusCode: 500);
});

// Seed DB if it doesn't exist already.
Directory.CreateDirectory(AppDbContext.DbPath);
using (IServiceScope scope = app.Services.CreateScope())
{
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

    bool nhlTeamsSeeded = dbContext.Teams.Any(x => x.LeagueId == Leagues.Nhl);
    if (!nhlTeamsSeeded)
        await NhlService.SeedTeams(dbContext, scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient());

    bool mlbTeamsSeeded = dbContext.Teams.Any(x => x.LeagueId == Leagues.Mlb);
    if (!mlbTeamsSeeded)
        await MlbService.SeedTeams(dbContext, scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient());

    bool cflTeamsSeeded = dbContext.Teams.Any(x => x.LeagueId == Leagues.Cfl);
    if (!cflTeamsSeeded)
        await CflService.SeedTeams(dbContext);
}

try
{
    Log.Information("Starting up");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
