namespace SpoilerFreeHighlights.Core.Endpoints;

public static class AllEndpoints
{
    public static Task<Team[]> GetTeams(AppDbContext dbContext)
    {
        return dbContext.Teams.ToArrayAsync();
    }

    public static async Task<Schedule?> GetGameDays(
        NhlService nhlService,
        MlbService mlbService,
        CflService cflService,
        ScheduleQuery scheduleQuery)
    {
        Dictionary<Leagues, LeagueService> services = new()
        {
            { Leagues.Nhl, nhlService },
            { Leagues.Mlb, mlbService },
            { Leagues.Cfl, cflService }
        };

        if (scheduleQuery.Leagues is null || scheduleQuery.Leagues.Length == 0 || scheduleQuery.Leagues.Contains(Leagues.All))
            scheduleQuery.Leagues = services.Keys.ToArray();

        List<Schedule> schedules = [];
        foreach (Leagues league in scheduleQuery.Leagues)
        {
            LeagueService leagueService = services[league];

            Schedule? leagueSchedule = await leagueService.GetScheduleForThisWeek(scheduleQuery.UserPreferences);
            if (leagueSchedule is null)
                return default;

            schedules.Add(leagueSchedule);
        }

        Schedule allSchedule = new()
        {
            League = Leagues.All,
            GameDays = schedules
                .SelectMany(x => x.GameDays)
                .GroupBy(x => x.DateLeague)
                .Select(x => new GameDay
                {
                    DateLeague = x.Key,
                    Games = x.SelectMany(y => y.Games).OrderBy(y => y.StartDateUtc).ToList()
                })
                .ToList()
        };

        return allSchedule;
    }

    public static YouTubePlaylist FetchPlaylistInfo(string playlistId, string channelId, int leagueId)
    {
        Leagues league = Leagues.FromValue(leagueId);
        YouTubePlaylist playlist = YouTubeService.FetchLatestVideosFromRssFeed(playlistId, channelId, league);
        playlist.Videos = [];
        return playlist;
    }

    public static async Task<LeagueConfigurationDto> GetAppSettings(AppDbContext dbContext)
    {
        return new() { LeagueConfigurations = await dbContext.LeagueConfigurations.IgnoreQueryFilters().ToListAsync() };
    }

    public static async Task<LeagueConfigurationDto> UpdateAppSettings(AppDbContext dbContext, LeagueConfigurationDto updatedLeagueConfigurationDto)
    {
        List<LeagueConfiguration> oldLeagueConfigs = await dbContext.LeagueConfigurations
            .IgnoreQueryFilters()
            .AsTracking()
            .ToListAsync();

        ComparisonResult<LeagueConfiguration> leagueConfigResults = oldLeagueConfigs.CompareWith(updatedLeagueConfigurationDto.LeagueConfigurations, x => x.LeagueId);
        dbContext.AddRange(leagueConfigResults.NewItems);
        dbContext.RemoveRange(leagueConfigResults.RemovedItems);

        foreach (var (existingLeagueConfigResult, updatedLeagueConfigResult) in leagueConfigResults.SameItems)
        {
            existingLeagueConfigResult.SelectPlaylistType = updatedLeagueConfigResult.SelectPlaylistType;

            ComparisonResult<PlaylistConfiguration> playlistConfigResults = existingLeagueConfigResult.Playlists.CompareWith(updatedLeagueConfigResult.Playlists, x => new { x.PlaylistId, x.ChannelId }); // vs x.Id

            foreach (var newPlaylistConfig in playlistConfigResults.NewItems)
                existingLeagueConfigResult.Playlists.Add(newPlaylistConfig);

            dbContext.RemoveRange(playlistConfigResults.RemovedItems);

            foreach (var (existingPlaylistConfigResult, updatedPlaylistConfigResult) in playlistConfigResults.SameItems)
            {
                existingPlaylistConfigResult.PlaylistId = updatedPlaylistConfigResult.PlaylistId;
                existingPlaylistConfigResult.PlaylistName = updatedPlaylistConfigResult.PlaylistName;
                existingPlaylistConfigResult.ChannelName = updatedPlaylistConfigResult.ChannelName;
                existingPlaylistConfigResult.ChannelId = updatedPlaylistConfigResult.ChannelId;
                existingPlaylistConfigResult.IsDisabled = updatedPlaylistConfigResult.IsDisabled;
                existingPlaylistConfigResult.RequiredVideoPartMatches = updatedPlaylistConfigResult.RequiredVideoPartMatches;
                existingPlaylistConfigResult.RequiredVideoTitlePercentageMatch = updatedPlaylistConfigResult.RequiredVideoTitlePercentageMatch;
                existingPlaylistConfigResult.TitlePattern = updatedPlaylistConfigResult.TitlePattern;
                existingPlaylistConfigResult.Comment = updatedPlaylistConfigResult.Comment;

                ComparisonResult<VideoTitleIdentifier> titleIdentifierResults = existingPlaylistConfigResult.TitleIdentifiers.CompareWith(updatedPlaylistConfigResult.TitleIdentifiers, x => x.Id);
                
                foreach (var newTitleIdentifier in titleIdentifierResults.NewItems)
                    existingPlaylistConfigResult.TitleIdentifiers.Add(newTitleIdentifier);
                
                dbContext.RemoveRange(titleIdentifierResults.RemovedItems);
                
                foreach (var (existingTitleIdentifierResult, updatedTitleIdentifierResult) in titleIdentifierResults.SameItems)
                    existingTitleIdentifierResult.Value = updatedTitleIdentifierResult.Value;

                ComparisonResult<VideoTitleTeamFormat> titleTeamFormatResults = existingPlaylistConfigResult.TeamFormats.CompareWith(updatedPlaylistConfigResult.TeamFormats, x => x.Id);

                foreach (var newTitleTeamFormat in titleTeamFormatResults.NewItems)
                    existingPlaylistConfigResult.TeamFormats.Add(newTitleTeamFormat);

                dbContext.RemoveRange(titleTeamFormatResults.RemovedItems);
                
                foreach (var (existingTitleTeamFormatResult, updatedTitleTeamFormatResult) in titleTeamFormatResults.SameItems)
                    existingTitleTeamFormatResult.Value = updatedTitleTeamFormatResult.Value;

                ComparisonResult<VideoTitleDateFormat> titleDateFormatResults = existingPlaylistConfigResult.DateFormats.CompareWith(updatedPlaylistConfigResult.DateFormats, x => x.Id);
                
                foreach (var newTitleDateFormat in titleDateFormatResults.NewItems)
                    existingPlaylistConfigResult.DateFormats.Add(newTitleDateFormat);
                
                dbContext.RemoveRange(titleDateFormatResults.RemovedItems);

                foreach (var (existingTitleDateFormatResult, updatedTitleDateFormatResult) in titleDateFormatResults.SameItems)
                    existingTitleDateFormatResult.Value = updatedTitleDateFormatResult.Value;
            }
        }

        await dbContext.SaveChangesAsync();

        return await GetAppSettings(dbContext);
    }

    public static async Task TestScheduleService(LeaguesService leaguesService)
    {
        await leaguesService.FetchAndCacheScheduledGames();
    }

    public static async Task TestYouTubeService(YouTubeService youtubeService)
    {
        bool newVideos = await youtubeService.FetchAndCacheNewVideos();
        if (newVideos)
            await youtubeService.AddYouTubeLinksToAllMatches();
    }

    public static async Task ResetData(AppDbContext dbContext)
    {
        await dbContext.YouTubeVideos.ExecuteDeleteAsync();
        await dbContext.YouTubePlaylists.ExecuteDeleteAsync();
        await dbContext.Games.ExecuteDeleteAsync();
        //await dbContext.Teams.ExecuteDeleteAsync();
        //await dbContext.Leagues.ExecuteDeleteAsync();
        await dbContext.SaveChangesAsync();
    }
}
