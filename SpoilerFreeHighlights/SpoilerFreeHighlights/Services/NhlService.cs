using SpoilerFreeHighlights.Shared.Models;
using System.Text.Json;

namespace SpoilerFreeHighlights.Services;

public class NhlService(HttpClient _httpClient)
{
    public async Task<NhlSchedule?> GetScheduleForThisWeek(DateOnly date)
    {
        string localCachePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Downloads", $"{date:yyyy-MM-dd} NHL.json");

        NhlSchedule? nhlSchedule = FileService.GetDataFromCache<NhlSchedule?>(localCachePath);
        if (nhlSchedule is null)
            nhlSchedule = await FetchScheduleDataFromNhlApi(localCachePath, date);

        return nhlSchedule;
    }

    public static Schedule ConvertFromNhlApiToUsableModels(NhlSchedule nhlSchedule)
    {
        GameWeek todaysGames = nhlSchedule.gameWeek[0];

        Schedule scheduleResults = new()
        {
            Games = todaysGames.games.Select(game => new GameInfo()
            {
                Id = (int)game.id,
                StartDateLocal = GetStartDateTime(game.startTimeUTC),
                StartDateUtc = game.startTimeUTC,
                IsScoreHidden = true,
                //YouTubeLink = ,
                HomeTeam = new TeamInfo()
                {
                    Id = game.homeTeam.id,
                    Name = game.homeTeam.commonName.@default,
                    Abbreviation = game.homeTeam.abbrev,
                    LogoLink = GetTeamLogo(game.homeTeam),
                    Score = game.homeTeam.score
                },
                AwayTeam = new TeamInfo()
                {
                    Id = game.awayTeam.id,
                    Name = game.awayTeam.commonName.@default,
                    Abbreviation = game.awayTeam.abbrev,
                    LogoLink = game.awayTeam.darkLogo,
                    Score = game.awayTeam.score
                }
            })
            .OrderBy(x => x.StartDateLocal)
            .ToList()
        };

        return scheduleResults;
    }

    private async Task<NhlSchedule?> FetchScheduleDataFromNhlApi(string localCachePath, DateOnly date)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"https://api-web.nhle.com/v1/schedule/{date:yyyy-MM-dd}");
        if (!response.IsSuccessStatusCode)
            return null;

        string resultJson = await response.Content.ReadAsStringAsync();

        Directory.CreateDirectory(Path.GetDirectoryName(localCachePath)!);
        File.WriteAllText(localCachePath, resultJson, FileService.JsonEncoding);

        return JsonSerializer.Deserialize<NhlSchedule>(resultJson);
    }

    private static DateTime GetStartDateTime(DateTime startTimeUTC)
    {
        DateTime newDateTime = TimeZoneInfo.ConvertTimeFromUtc(startTimeUTC, TimeZoneInfo.Local);
        return newDateTime;
    }

    private static string GetTeamLogo(Team team)
    {
        //if (team.commonName.@default == "Capitals" && team.placeName.@default == "Washington")
        //    return @"Resources\Team Logos\washington_capitals.svg";

        return team.darkLogo;
    }
}
