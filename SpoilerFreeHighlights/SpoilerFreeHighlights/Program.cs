using MudBlazor;
using MudBlazor.Services;
using SpoilerFreeHighlights.Components;
using SpoilerFreeHighlights.Services;
using SpoilerFreeHighlights.Shared.Models;

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

builder.Services.AddScoped<NhlService>();
builder.Services.AddScoped<YouTubeService>();

builder.Services.AddMudServices();

MudGlobal.UnhandledExceptionHandler = (exception) => Console.WriteLine(exception);

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

app.MapGet("/api/GetGames", async (NhlService nhlService, YouTubeService youTubeService, DateOnly? gameDay = default) =>
{
    gameDay ??= DateOnly.FromDateTime(DateTime.Now);

    Console.WriteLine($"Fetching NHL schedule for {gameDay:yyyy-MM-dd}...");
    NhlSchedule? nhlSchedule = await nhlService.GetScheduleForThisWeek(gameDay.Value);
    if (nhlSchedule is null)
        return Results.Problem("Failed to retrieve data from external API.", statusCode: 500);

    Console.WriteLine("Converting NHL API data to usable models...");
    Schedule scheduleResults = NhlService.ConvertFromNhlApiToUsableModels(nhlSchedule);

    Console.WriteLine("Populating YouTube links for each game...");
    await youTubeService.PopulateYouTubeLinks(scheduleResults, "NHL", gameDay.Value);

    Console.WriteLine("Returning schedule results.");
    return scheduleResults is not null
        ? Results.Ok(scheduleResults)
        : Results.Problem("Failed to deserialize external API response.", statusCode: 500);
});

app.Run();
