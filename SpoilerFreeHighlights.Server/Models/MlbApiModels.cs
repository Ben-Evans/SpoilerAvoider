namespace SpoilerFreeHighlights.Server.Models;

#region MLB Schedule API Models
public class MlbApiSchedule
{
    public string Copyright { get; set; }
    public int TotalItems { get; set; }
    public int TotalEvents { get; set; }
    public int TotalGames { get; set; }
    public int TotalGamesInProgress { get; set; }
    public List<MlbApiDate> Dates { get; set; }
}

public class MlbApiDate
{
    public string Date { get; set; }
    public int TotalItems { get; set; }
    public int TotalEvents { get; set; }
    public int TotalGames { get; set; }
    public int TotalGamesInProgress { get; set; }
    public List<MlbApiGame> Games { get; set; }
    public List<object> Events { get; set; }
}

public class MlbApiGame
{
    public int GamePk { get; set; }
    public string GameGuid { get; set; }
    public string Link { get; set; }
    public string GameType { get; set; }
    public string Season { get; set; }
    public DateTime GameDate { get; set; }
    public string OfficialDate { get; set; }
    public MlbApiStatus Status { get; set; }
    public MlbApiTeams Teams { get; set; }
    public MlbApiVenue Venue { get; set; }
    public MlbApiContent Content { get; set; }
    public int GameNumber { get; set; }
    public bool PublicFacing { get; set; }
    public string DoubleHeader { get; set; }
    public string GamedayType { get; set; }
    public string Tiebreaker { get; set; }
    public string CalendarEventID { get; set; }
    public string SeasonDisplay { get; set; }
    public string DayNight { get; set; }
    public string Description { get; set; }
    public int ScheduledInnings { get; set; }
    public bool ReverseHomeAwayStatus { get; set; }
    public int InningBreakLength { get; set; }
    public int GamesInSeries { get; set; }
    public int SeriesGameNumber { get; set; }
    public string SeriesDescription { get; set; }
    public string RecordSource { get; set; }
    public string IfNecessary { get; set; }
    public string IfNecessaryDescription { get; set; }
}

public class MlbApiStatus
{
    public string AbstractGameState { get; set; }
    public string CodedGameState { get; set; }
    public string DetailedState { get; set; }
    public string StatusCode { get; set; }
    public bool StartTimeTBD { get; set; }
    public string AbstractGameCode { get; set; }
}

public class MlbApiTeams
{
    public MlbApiTeamInfo Away { get; set; }
    public MlbApiTeamInfo Home { get; set; }
}

public class MlbApiTeamInfo
{
    public MlbApiLeagueRecord LeagueRecord { get; set; }
    public MlbApiBasicTeam Team { get; set; }
    public bool SplitSquad { get; set; }
    public int SeriesNumber { get; set; }
}

public class MlbApiLeagueRecord
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public string Pct { get; set; }
}

public class MlbApiBasicTeam
{
    public int Id { get; set; }
    // "Seattle Mariners" vs "Toronto Blue Jays" or "LAD/SEA" vs "TOR/LAD"
    public string Name { get; set; }
    public string Link { get; set; }
}

public class MlbApiVenue
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }
}

public class MlbApiContent
{
    public string Link { get; set; }
}
#endregion MLB Schedule API Models

#region MLB Team API Models
public record MlbApiTeamResponse(
    string Copyright,
    List<MlbApiTeam> Teams
);

public record ApiReference(
    int Id,
    string Name,
    string Link,
    string? Abbreviation = null
);

public record VenueReference(
    int Id,
    string Name,
    string Link
);

public record SpringVenueReference(
    int Id,
    string Link
);

public record MlbApiTeam(
    ApiReference SpringLeague,
    string AllStarStatus,
    int Id,
    string Name,
    string Link,
    int Season,
    VenueReference Venue,
    SpringVenueReference SpringVenue,
    string TeamCode,
    string FileCode,
    string Abbreviation,
    string TeamName,
    string LocationName,
    string FirstYearOfPlay,
    ApiReference League,
    ApiReference Division,
    ApiReference Sport,
    string ShortName,
    string FranchiseName,
    string ClubName,
    bool Active
);
#endregion MLB Team API Models
