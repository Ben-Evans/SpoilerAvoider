namespace SpoilerFreeHighlights.Models;

public record NhlSchedule(
    string nextStartDate,
    string previousStartDate,
    List<GameWeek> gameWeek
);

public record GameWeek(
    DateOnly date,
    string dayAbbrev,
    int numberOfGames,
    List<object> datePromo,
    List<Game> games
);

public record Game(
    long id,
    int season,
    int gameType,
    Venue venue,
    bool neutralSite,
    DateTime startTimeUTC,
    string easternUTCOffset,
    string venueUTCOffset,
    string venueTimezone,
    string gameState,
    string gameScheduleState,
    List<TvBroadcast> tvBroadcasts,
    Team awayTeam,
    Team homeTeam,
    PeriodDescriptor periodDescriptor,
    GameOutcome gameOutcome,
    PlayerStat winningGoalie,
    PlayerStat winningGoalScorer,
    string threeMinRecap,
    string threeMinRecapFr,
    string condensedGame,
    string condensedGameFr,
    string gameCenterLink
);

public record LocalizedString(
    string @default,
    string? fr = null
);

public record Venue(
    string @default
);

public record TvBroadcast(
    int id,
    string market,
    string countryCode,
    string network,
    int sequenceNumber
);

public record Team(
    int id,
    LocalizedString commonName,
    LocalizedString placeName,
    LocalizedString placeNameWithPreposition,
    string abbrev,
    string logo,
    string darkLogo,
    bool awaySplitSquad,
    bool homeSplitSquad,
    int score
);

public record PeriodDescriptor(
    int number,
    string periodType,
    int maxRegulationPeriods
);

public record GameOutcome(
    string lastPeriodType
);

public record PlayerStat(
    int playerId,
    LocalizedString firstInitial,
    LocalizedString lastName
);
