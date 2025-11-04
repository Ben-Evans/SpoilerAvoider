namespace SpoilerFreeHighlights.Shared.Models;

public class ScheduleQuery
{
    public DateTime UserDateTimeToday { get; } = DateTime.Today;
    public DateOnly UserToday => DateOnly.FromDateTime(UserDateTimeToday);

    public Leagues[] Leagues { get; set; } = [ Enums.Leagues.All ];

    public UserPreference UserPreferences { get; set; } = new();
}

// TODO: Handle all teams vs no teams
public class UserPreference
{
    public UserPreference()
    {
        Leagues[] leagues = Leagues.GetAllLeagues();
        foreach (Leagues league in leagues)
            LeaguePreferences.Add(league, []);
    }

    public Dictionary<Leagues, List<Team>> LeaguePreferences { get; set; } = [];
}

public class NotificationPreferences
{
}
