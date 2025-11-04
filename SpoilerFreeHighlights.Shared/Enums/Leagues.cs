namespace SpoilerFreeHighlights.Shared.Enums;

using Ardalis.SmartEnum;
using Ardalis.SmartEnum.SystemTextJson;
using System;
using System.Text.Json.Serialization;

[JsonConverter(typeof(SmartEnumNameConverter<Leagues, int>))]
public sealed class Leagues : SmartEnum<Leagues>
{
    private static readonly TimeZoneInfo DefaultTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York"); //"Eastern Standard Time");

    public static readonly Leagues All = new(nameof(All), 0, DefaultTimeZone);
    public static readonly Leagues Nhl = new(nameof(Nhl), 1, DefaultTimeZone);
    public static readonly Leagues Mlb = new(nameof(Mlb), 2, DefaultTimeZone);
    public static readonly Leagues Cfl = new(nameof(Cfl), 3, DefaultTimeZone);

    /// <summary>
    /// Based on the head office, media operations, official schedules, standings, and announcements.
    /// </summary>
    public TimeZoneInfo TimeZone { get; }
    public DateTime LeagueDateTimeNow { get; }
    public DateTime LeagueDateTimeToday { get; }
    public DateOnly LeagueDateToday { get; }

    public static Leagues[] GetAllLeagues(bool excludeAll = true)
    {
        IReadOnlyCollection<Leagues> leagues = List;
        if (excludeAll)
            leagues = leagues.Where(x => x != All).ToArray();

        return leagues.OrderBy(x => x.Value).ToArray();
    }

    private Leagues(string name, int value, TimeZoneInfo timeZone) : base(name, value)
    {
        TimeZone = timeZone;
        LeagueDateTimeNow = DateTimeService.GetLeagueDateTime(this);
        LeagueDateTimeToday = LeagueDateTimeNow.Date;
        LeagueDateToday = DateOnly.FromDateTime(LeagueDateTimeNow);
    }
}
