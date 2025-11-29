using System.Net.Http.Json;

namespace SpoilerFreeHighlights.BlazorClient.Pages;

public partial class ScheduleMatchups // : IDisposable
{
    [Inject] public required HttpClient HttpClient { get; set; }
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required IDialogService DialogService { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }
    [Inject] private IServiceProvider Services { get; set; } = default!;
    [Inject] private ILogger<ScheduleMatchups> Logger { get; set; } = default!;

    [Parameter] public string? PageRoute { get; set; } = string.Empty;
    [Parameter] public Leagues[] SelectedLeagues { get; set; } = [ Leagues.All ];

    private Schedule? schedule;

    private LocalCacheService? LocalStorage;
    private UserPreference userPreferences = new();
    //private Timer? refreshTimer;

    private string? previousPageRoute;

    protected override async Task OnParametersSetAsync()
    {
        if (!string.Equals(previousPageRoute, PageRoute, StringComparison.OrdinalIgnoreCase))
        {
            previousPageRoute = PageRoute;

            await InitializeSafely();
        }
    }

    protected static string GetTeamLogoLink(Team team)
    {
        if (team.LeagueId == Leagues.Nhl && team.ToString() == "Washington Capitals")
            return $"/Resources/Team-Logos/{Leagues.Nhl.DisplayName}/Resized/{team.Abbreviation}.svg";
        else if (team.LeagueId == Leagues.Cfl)
            return $"/Resources/Team-Logos/{Leagues.Cfl.DisplayName}/Originals/{team.Abbreviation}.svg";

        return team.LogoLink;
    }

    protected Task OpenYouTubeLink(string link)
    {
        if (string.IsNullOrEmpty(link))
            return Task.CompletedTask;

        DialogOptions dialogOptions = new()
        {
            CloseOnEscapeKey = true,
            BackgroundClass = "dialog-popup",
            NoHeader = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Large,
            CloseButton = true
        };

        // "https:www.youtube.com/watch?v=STBSUasnJ5s" => "https:www.youtube.com/embed/STBSUasnJ5s"
        string ytLink = link.Replace("watch?v=", "embed/").Replace("youtube.com", "youtube-nocookie.com") + "?autoplay=1&rel=0";
        DialogParameters dialogParameters = new()
        {
            { "YouTubeVideoLink", ytLink }
        };

        return DialogService.ShowAsync<Highlights>("View Highlights", dialogParameters, dialogOptions);
    }

    /// <summary>
    /// Setting the LocalCacheService this way because it can't be found on page refresh likely due to pre-rendering.
    /// </summary>
    private async Task InitializeSafely()
    {
        string pagePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

        Leagues[] allLeagues = Leagues.GetAllLeagues();
        Leagues? league = allLeagues.FirstOrDefault(x => pagePath.Equals(x.DisplayName, StringComparison.OrdinalIgnoreCase));
        if (league is not null)
            SelectedLeagues = [ league ];

        LocalStorage = Services.GetRequiredService<LocalCacheService>();
        userPreferences = await LocalStorage.GetUserPreferences();

        await FetchScheduleData();

        //refreshTimer = new Timer(async _ => await RefreshData(), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Loads the schedule data based on the current <see cref="SelectedDate" /> and <see cref="SelectedLeagues" />.
    /// </summary>
    private async Task FetchScheduleData()
    {
        try
        {
            ScheduleQuery scheduleQuery = new()
            {
                Leagues = SelectedLeagues,
                UserPreferences = userPreferences
            };
            HttpResponseMessage? response = await HttpClient.PostAsJsonAsync("/api/GetGameDays", scheduleQuery);
            if (response.IsSuccessStatusCode)
                schedule = await response.Content.ReadFromJsonAsync<Schedule>();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to fetch data from server.", Severity.Error);

            Logger.LogError($"Error fetching schedule: {ex.Message}");
        }
    }

    /*private async Task RefreshData()
    {
        //Logger.LogDebug("Refreshing data: {Time}", DateTime.Now.ToString("HH:mm"));
        Console.WriteLine($"Refreshing data: {DateTime.Now.ToString("HH:mm")}");

        await FetchScheduleData();

        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        refreshTimer?.Dispose();

        GC.SuppressFinalize(this);
    }*/
}
