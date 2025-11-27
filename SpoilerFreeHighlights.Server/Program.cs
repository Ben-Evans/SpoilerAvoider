using Microsoft.AspNetCore.Mvc;
using SpoilerFreeHighlights.Core.Endpoints;
using SpoilerFreeHighlights.Core.Services.LeagueServices;
using SpoilerFreeHighlights.Server.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

Environment.SetEnvironmentVariable("APP_BASE_DIR", AppContext.BaseDirectory, EnvironmentVariableTarget.Process);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddRazorComponents();

builder.Host.UseSerilog();

builder.Services.AddHttpClient("Default", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue("BaseAddress", "https://localhost:7137"));
});

builder.Services.SetupDatabase(builder.Configuration, false);

builder.Services.AddServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowBlazorClient", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetValue("BlazorClientBaseAddress", "https://localhost:7107"))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddHostedService<LeagueScheduleRefreshService>();
builder.Services.AddHostedService<YouTubeRefreshService>();
builder.Services.AddHostedService<DataCleanupService>();

var app = builder.Build();

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
app.UseCors("AllowBlazorClient");
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapGet("/api/reset", (AppDbContext dbContext) => AllEndpoints.ResetData(dbContext));

app.MapGet("/api/test/s", (LeaguesService leaguesService) => AllEndpoints.TestScheduleService(leaguesService));

app.MapGet("/api/test/yt", (YouTubeService youtubeService) => AllEndpoints.TestYouTubeService(youtubeService));

app.MapGet("/api/GetTeams", (AppDbContext dbContext) => AllEndpoints.GetTeams(dbContext));

app.MapPost("/api/GetGameDays", async (NhlService nhlService, MlbService mlbService, CflService cflService, [FromBody] ScheduleQuery scheduleQuery) =>
{
    Schedule? allSchedule = await AllEndpoints.GetGameDays(nhlService, mlbService, cflService, scheduleQuery);

    return allSchedule is not null
        ? Results.Ok(allSchedule)
        : Results.Problem("Failed to deserialize external API response.", statusCode: 500);
});

app.MapGet("/api/FetchPlaylistInfo", ([FromQuery] string playlistId, [FromQuery] string channelId, [FromQuery] int leagueId) => AllEndpoints.FetchPlaylistInfo(playlistId, channelId, leagueId));

app.MapGet("/api/GetAppSettings", (AppDbContext dbContext) => AllEndpoints.GetAppSettings(dbContext));

app.MapPut("/api/UpdateAppSettings", (AppDbContext dbContext, [FromBody] LeagueConfigurationDto updatedConfigDto) => AllEndpoints.UpdateAppSettings(dbContext, updatedConfigDto));

await app.Services.ApplyMigrationsAndSeedDatabase(false);

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
