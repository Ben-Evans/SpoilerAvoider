namespace SpoilerFreeHighlights.Core.Models;

public record NhlApiSchedule(
    string NextStartDate,
    string PreviousStartDate,
    List<NhlApiGameWeek> GameWeek
);

public record NhlApiGameWeek(
    DateOnly Date,
    string DayAbbrev,
    int NumberOfGames,
    List<object> DatePromo,
    List<NhlApiGame> Games
);

public record NhlApiGame(
    long Id,
    int Season,
    int GameType,
    Venue Venue,
    bool NeutralSite,
    DateTime StartTimeUTC,
    string EasternUTCOffset,
    string VenueUTCOffset,
    string VenueTimezone,
    string GameState,
    string GameScheduleState,
    List<NhlApiTvBroadcast> TvBroadcasts,
    NhlApiTeam AwayTeam,
    NhlApiTeam HomeTeam,
    NhlApiPeriodDescriptor PeriodDescriptor,
    NhlApiGameOutcome GameOutcome,
    NhlApiPlayerStat WinningGoalie,
    NhlApiPlayerStat WinningGoalScorer,
    string ThreeMinRecap,
    string ThreeMinRecapFr,
    string CondensedGame,
    string CondensedGameFr,
    string GameCenterLink
);

public record LocalizedString(
    string Default,
    string Fr
);

public record Venue(
    string Default
);

public record NhlApiTvBroadcast(
    int Id,
    string Market,
    string CountryCode,
    string Network,
    int SequenceNumber
);

public record NhlApiTeam(
    int Id,
    LocalizedString CommonName,
    LocalizedString PlaceName,
    LocalizedString PlaceNameWithPreposition,
    string Abbrev,
    string Logo,
    string DarkLogo,
    bool AwaySplitSquad,
    bool HomeSplitSquad,
    int Score
);

public record NhlApiPeriodDescriptor(
    int Number,
    string PeriodType,
    int MaxRegulationPeriods
);

public record NhlApiGameOutcome(
    string LastPeriodType
);

public record NhlApiPlayerStat(
    int PlayerId,
    LocalizedString FirstInitial,
    LocalizedString LastName
);
