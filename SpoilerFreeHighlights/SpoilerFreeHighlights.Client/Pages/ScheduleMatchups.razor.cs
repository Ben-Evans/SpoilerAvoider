using System.Net.Http.Json;

namespace SpoilerFreeHighlights.Client.Pages;

public partial class ScheduleMatchups
{
    [Inject] public required HttpClient HttpClient { get; set; }

    private List<GameInfo> games = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<Schedule>($"/api/GetGames"); // ?date={DateTime.Now.AddDays(-1):yyyy-MM-dd}

            games = response.Games;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching schedule: {ex.Message}");
            games = new();
        }
    }

    protected static string GetTeamLogoSizing(TeamInfo team)
    {
        if (team.Abbreviation == "WSH")
            return "15%";

        return "50%";
    }
}
