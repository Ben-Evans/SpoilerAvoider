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
            var response = await HttpClient.GetFromJsonAsync<Schedule>("/api/GetGames");

            games = response.Games;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching schedule: {ex.Message}");
            games = new();
        }
    }
}

/*private void ToggleScore(int gameId)
{
    var game = games.FirstOrDefault(g => g.Id == gameId);
    if (game != null)
    {
        // Assuming you add a 'public bool ShowScore { get; set; }' property to your Game model
        game.ShowScore = !game.ShowScore;
    }
}

private string GetHighlightLink(int gameId)
{
    // This is a placeholder. A direct link to a video by game ID is not public.
    // You would likely link to the Sportsnet NHL Highlights playlist:
    // https://www.youtube.com/@Sportsnet
    return "https://www.youtube.com/@Sportsnet/videos";
}*/