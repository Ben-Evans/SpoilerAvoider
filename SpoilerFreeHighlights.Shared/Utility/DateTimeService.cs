namespace SpoilerFreeHighlights.Shared.Utility;

public static class DateTimeService
{
    public static DateTime GetLeagueDateTime(Leagues league) => ConvertToLeagueDateTime(DateTime.UtcNow, league);
    public static DateTime ConvertToLeagueDateTime(this DateTime utcTime, Leagues league) => TimeZoneInfo.ConvertTimeFromUtc(utcTime, league.TimeZone);
}
