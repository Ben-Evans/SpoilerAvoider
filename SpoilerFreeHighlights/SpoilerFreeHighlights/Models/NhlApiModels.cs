namespace SpoilerFreeHighlights.Models;

// today's games
/*.gameWeek[0].games[i]
  .awayTeam
    .abbrev
    .darkLogo
    .score
  .homeTeam
    .abbrev
    .darkLogo
    .score
  .periodDescriptor
    .number
    .periodType*/
// 1. The root model for the entire API response
public record NhlSchedule(
    string nextStartDate,
    string previousStartDate,
    List<GameWeek> gameWeek
);

// 2. Represents a single day's schedule, found within the 'gameWeek' array
public record GameWeek(
    DateOnly date,
    string dayAbbrev,
    int numberOfGames,
    // Note: datePromo is an empty array in the sample, but is included for completeness
    List<object> datePromo,
    List<Game> games
);

// 3. Represents a single NHL game
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

// 4. Shared model for multilingual text fields (used in team and player names)
public record LocalizedString(
    string @default, // '@' is used as 'default' is a C# keyword
    string? fr = null
);

// 5. Details about the venue
public record Venue(
    string @default
);

// 6. Details for a TV broadcast
public record TvBroadcast(
    int id,
    string market,
    string countryCode,
    string network,
    int sequenceNumber
);

// 7. Details for a home or away team
public record Team(
    int id,
    LocalizedString commonName,
    LocalizedString placeName,
    LocalizedString placeNameWithPreposition,
    string abbrev,
    string logo,
    string darkLogo,
    bool awaySplitSquad, // The property 'awaySplitSquad' is in the 'awayTeam'
    bool homeSplitSquad, // The property 'homeSplitSquad' is in the 'homeTeam'
    int score
);

// 8. Information about the period descriptor
public record PeriodDescriptor(
    int number,
    string periodType,
    int maxRegulationPeriods
);

// 9. Information about the game outcome
public record GameOutcome(
    string lastPeriodType
);

// 10. Details for a player stat (Winning Goalie/Goal Scorer)
public record PlayerStat(
    int playerId,
    LocalizedString firstInitial,
    LocalizedString lastName
);
