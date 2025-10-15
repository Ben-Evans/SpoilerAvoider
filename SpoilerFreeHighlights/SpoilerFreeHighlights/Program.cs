using SpoilerFreeHighlights.Components;
using SpoilerFreeHighlights.Shared.Models;
using System;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddInteractiveServerComponents();

// Register HttpClient for Server and Client DI.
builder.Services.AddHttpClient("Default", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue("BaseAddress", "https://localhost:7137/"));
});
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Default"));

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

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(SpoilerFreeHighlights.Client._Imports).Assembly);

/*
 * var apiUrl = $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId={yourPlaylistId}&key={yourApiKey}&maxResults=50";
// Use HttpClient on the Server-side to make this request
// var response = await httpClient.GetFromJsonAsync<YouTubeApiPlaylistResponse>(apiUrl);
 */
/*app.MapGet("/users/{id}", (int id, IUserService userService) =>
{
    var user = userService.GetUser(id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
});*/
//app.MapGet("/games", async (IHttpClientFactory clientFactory) =>
app.MapGet("/api/GetGames", async (HttpClient httpClient) =>
{
    Encoding nhlEncoding = Encoding.Unicode;
    string date = DateTime.Now.ToString("yyyy-MM-dd");
    string localCachePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Downloads", $"{date} NHL.json");
    string cachedFileContents = string.Empty;
    NhlSchedule? nhlApiResults;

    try
    {
        if (File.Exists(localCachePath))
            cachedFileContents = File.ReadAllText(localCachePath, nhlEncoding);
    }
    catch (Exception)
    {
        // File doesn't exist.
    }
    
    if (string.IsNullOrWhiteSpace(cachedFileContents))
    {
        HttpResponseMessage response = await httpClient.GetAsync($"https://api-web.nhle.com/v1/schedule/{date}");

        if (!response.IsSuccessStatusCode)
            return Results.StatusCode((int)response.StatusCode);

        string resultJson = await response.Content.ReadAsStringAsync();

        Directory.CreateDirectory(Path.GetDirectoryName(localCachePath)!);
        File.WriteAllText(localCachePath, resultJson, nhlEncoding);

        //results = await response.Content.ReadFromJsonAsync<NhlSchedule>();
        nhlApiResults = JsonSerializer.Deserialize<NhlSchedule>(resultJson);
    }
    else
    {
        nhlApiResults = JsonSerializer.Deserialize<NhlSchedule>(cachedFileContents);
    }

    // Map from NhlSchedule to Schedule
    GameWeek todaysGames = nhlApiResults.gameWeek[0];
    var usableScheduleResults = new Schedule()
    {
        Games = todaysGames.games.Select(game => new GameInfo()
        {
            Id = (int)game.id,
            //StartDate = TimeZoneInfo.ConvertTimeFromUtc(game.startTimeUTC),
            IsScoreHidden = true,
            HomeTeam = new TeamInfo()
            {
                Id = game.homeTeam.id,
                Name = game.homeTeam.commonName.@default,
                Abbreviation = game.homeTeam.abbrev,
                LogoLink = game.homeTeam.darkLogo,
                Score = game.homeTeam.score
            },
            AwayTeam = new TeamInfo()
            {
                Id = game.awayTeam.id,
                Name = game.awayTeam.commonName.@default,
                Abbreviation = game.awayTeam.abbrev,
                LogoLink = game.awayTeam.darkLogo,
                Score = game.awayTeam.score
            }
        }).ToList()
    };

    return usableScheduleResults is not null
        ? Results.Ok(usableScheduleResults)
        : Results.Problem("Failed to deserialize external API response.", statusCode: 500);
});

app.Run();
