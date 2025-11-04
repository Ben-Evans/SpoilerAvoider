using FuzzySharp;
using Microsoft.Extensions.Options;
using System.ServiceModel.Syndication;
using System.Xml;

namespace SpoilerFreeHighlights.Services;

public class YouTubeRssService(
    AppDbContext _dbContext,
    IConfiguration _configuration,
    IOptions<YouTubeSettings> _youtubeConfigOptions)
{
    private static readonly ILogger _logger = Log.ForContext<YouTubeRssService>();

    public async Task<YouTubePlaylist[]> FetchAndAddLatestYouTubeRssDataForLeague(Schedule schedule)
    {
        if (schedule.League == Leagues.All)
            throw new ArgumentException("League must be specified to fetch YouTube RSS data.");

        if (!_youtubeConfigOptions.Value.LeaguePlaylists.TryGetValue(schedule.League.Name, out LeaguePlaylistSettings? playlistSettings) || playlistSettings is null)
            throw new Exception($"No properly formatted playlist settings found for league '{schedule.League.Name}'.");

        List<PlaylistConfiguration> playlistConfigs = new();
        string playlistSelectionMode = playlistSettings.SelectPlaylist;
        switch (playlistSelectionMode)
        {
            case "All":
                playlistConfigs.AddRange(playlistSettings.Playlists);
                break;
            case "Single":
            case "First":
                playlistConfigs.Add(playlistSettings.Playlists.First());
                break;
            default:
                throw new NotImplementedException($"Unrecognized or missing playlist selection mode for league '{schedule.League.Name}'.");
        }

        //List<YouTubePlaylist> finalPlaylists = new();

        foreach (PlaylistConfiguration playlistConfig in playlistConfigs)
        {
            YouTubePlaylist newPlaylist = FetchLatestVideosFromRssFeed(playlistConfig.Id, schedule.League);

            // TODO: Ensure this is proper...
            // Discard any shorts or videos with unlikely dates
            //int reasonablePublishDate = (video.PublishedDate >= game.StartDateUtc && video.PublishedDate < game.StartDateUtc.AddDays(2)) ? 1 : 0;
            DateTime leagueDateTimeToday = schedule.League.LeagueDateTimeToday;
            newPlaylist.Videos = newPlaylist.Videos
                .Where(x => !x.Link.Contains("/shorts/") && x.PublishedDateTimeLeague >= leagueDateTimeToday && x.PublishedDateTimeLeague < leagueDateTimeToday.AddDays(8))
                .ToList();

            if (!newPlaylist.Videos.Any())
                continue;

            YouTubePlaylist? existingPlaylist = await _dbContext.YouTubePlaylists
                .Include(r => r.Videos)
                .FirstOrDefaultAsync(r => r.Id == playlistConfig.Id);
            if (existingPlaylist is null)
            {
                // **Scenario A: Playlist is NEW**
                _dbContext.YouTubePlaylists.Add(newPlaylist);
                // After saving, use the added entity for the final result
                //finalPlaylists.Add(newPlaylist);
            }
            else
            {
                // **Scenario B: Playlist EXISTS**
                string[] existingVideoIds = existingPlaylist.Videos.Select(v => v.Id).ToArray();

                // 2. We use the overload that compares the source (newPlaylist.Videos)
                //    against a collection of simple keys (existingVideoIds).
                YouTubeVideo[] newVideosToAdd = newPlaylist.Videos
                    .ExceptBy(existingVideoIds, v => v.Id)
                    .ToArray();

                existingPlaylist.Videos.AddRange(newVideosToAdd);

                _dbContext.YouTubeVideos.AddRange(newVideosToAdd);

                // The 'existingPlaylist' is now updated and will be returned
                //finalPlaylists.Add(existingPlaylist);
            }

            // 4. Save changes once per playlist after all additions
            await _dbContext.SaveChangesAsync();
        }


        //YouTubePlaylist matchedPlaylist = MatchVideosToGames(schedule, fetchedPlaylist, playlistConfig);



        List<YouTubePlaylist> finalPlaylists = new();
        return finalPlaylists.ToArray();
    }

    /// <summary>
    /// For each league x league playlist:
    /// - Check for new videos and save references for ones that might be highlights.
    /// - Set game links if ready.
    /// </summary>
    public async Task FetchAndCacheNewVideos()
    {
        foreach (Leagues league in Leagues.GetAllLeagues())
        {
            List<PlaylistConfiguration> playlistConfigs = GetPlaylistConfigurations(league);
            foreach (PlaylistConfiguration playlistConfig in playlistConfigs)
            {
                YouTubePlaylist newPlaylist = FetchLatestVideosFromRssFeed(playlistConfig.Id, league);

                DateTime daysForward = league.LeagueDateTimeNow.AddDays(_configuration.GetValue<int>("FetchDaysForwards"));
                DateTime daysBack = league.LeagueDateTimeNow.AddDays(-_configuration.GetValue<int>("FetchDaysBack"));

                // Discard any shorts or videos with unlikely dates
                newPlaylist.Videos = newPlaylist.Videos
                    .Where(x => !x.Link.Contains("/shorts/") && x.PublishedDateTimeLeague >= daysBack && x.PublishedDateTimeLeague < daysForward)
                    .ToList();

                if (!newPlaylist.Videos.Any())
                    continue;

                YouTubePlaylist? existingPlaylist = await _dbContext.YouTubePlaylists
                    .Include(x => x.Videos)
                    .FirstOrDefaultAsync(x => x.Id == playlistConfig.Id);

                // **Scenario A: Playlist is NEW**
                if (existingPlaylist is null)
                {
                    _dbContext.YouTubePlaylists.Add(newPlaylist);
                    // After saving, use the added entity for the final result
                    //finalPlaylists.Add(newPlaylist);
                }
                // **Scenario B: Playlist EXISTS**
                else
                {
                    string[] existingVideoIds = existingPlaylist.Videos.Select(x => x.Id).ToArray();

                    YouTubeVideo[] newVideosToAdd = newPlaylist.Videos
                        .ExceptBy(existingVideoIds, x => x.Id)
                        .ToArray();

                    existingPlaylist.Videos.AddRange(newVideosToAdd);

                    _dbContext.YouTubeVideos.AddRange(newVideosToAdd);
                }

                await _dbContext.SaveChangesAsync();
            }
        }
    }

    public async Task AddYouTubeLinksToAllMatches()
    {
        foreach (Leagues league in Leagues.GetAllLeagues())
            await AddYouTubeLinksToMatches(league);
    }

    public async Task AddYouTubeLinksToMatches(Leagues league)
    {
        List<PlaylistConfiguration> playlistConfigs = GetPlaylistConfigurations(league);
        string[] playlistIds = playlistConfigs.Select(x => x.Id).ToArray();

        YouTubePlaylist[] playlists = await _dbContext.YouTubePlaylists
            .AsTracking()
            .Include(x => x.Videos)
            .Where(x => x.LeagueId == league.Value && playlistIds.Contains(x.Id))
            .ToArrayAsync();

        Game[] games = await _dbContext.Games
            .AsTracking()
            .Where(x => x.LeagueId == league.Value && string.IsNullOrEmpty(x.YouTubeLink))
            .ToArrayAsync();

        bool saveChanges = false;

        foreach (YouTubePlaylist playlist in playlists)
        {
            PlaylistConfiguration playlistConfig = playlistConfigs.First(x => x.Id == playlist.Id);

            int requiredVideoTitleMatchPercentage = playlistConfig.RequiredVideoTitlePercentageMatch;
            foreach (Game game in games)
            {
                if (!string.IsNullOrEmpty(game.YouTubeLink))
                    continue;

                // "NHL Highlights | Devils vs. Maple Leafs - October 21, 2025";
                string expectedTitleA = $"NHL Highlights | {game.HomeTeam.Name} vs. {game.AwayTeam.Name} - {game.StartDateLeagueTime:MMMM d, yyyy}";
                string expectedTitleB = $"NHL Highlights | {game.AwayTeam.Name} vs. {game.HomeTeam.Name} - {game.StartDateLeagueTime:MMMM d, yyyy}";

                foreach (YouTubeVideo video in playlist.Videos)
                {
                    bool titleDateMatch1 = video.Title.Contains(game.StartDateLeagueTime.ToString("MMMM d, yyyy"));
                    bool titleDateMatch2 = video.Title.Contains(game.StartDateLeagueTime.ToString("MMM d, yyyy"));
                    bool titleDateMatch = titleDateMatch1 || titleDateMatch2;
                    if (!titleDateMatch)
                        continue;

                    // Allow highlight from game day and the next day.
                    bool publishDateMatch = video.PublishedDateTimeLeague >= game.StartDateLeagueTime && video.PublishedDateTimeLeague.AddDays(1) <= game.StartDateLeagueTime;
                    if (!publishDateMatch)
                        continue;

                    int percentageMatchA = Fuzz.Ratio(expectedTitleA, video.Title);
                    int percentageMatchB = Fuzz.Ratio(expectedTitleB, video.Title);
                    bool titleMatch = percentageMatchA >= requiredVideoTitleMatchPercentage || percentageMatchB >= requiredVideoTitleMatchPercentage;
                    if (titleMatch)
                    {
                        saveChanges = true;

                        game.YouTubeLink = video.Link;
                        break;
                    }
                }
            }
        }

        if (saveChanges)
        {
            //_dbContext.Update();
            await _dbContext.SaveChangesAsync();
        }
    }

    /*public static YouTubePlaylist AddYouTubeLinksToMatches(
        Schedule schedule,
        YouTubePlaylist highlightPlaylist,
        PlaylistConfiguration config)
    {
        Game[] games = schedule.GameDays
            .SelectMany(x => x.Games)
            .ToArray();

        bool saveChanges = false;

        int requiredVideoTitleMatchPercentage = config.RequiredVideoTitlePercentageMatch;
        foreach (Game game in games)
        {
            if (!string.IsNullOrEmpty(game.YouTubeLink))
                continue;

            //string expectedTitleA = "NHL Highlights | Devils vs. Maple Leafs - October 21, 2025";
            string expectedTitleA = $"NHL Highlights | {game.HomeTeam.Name} vs. {game.AwayTeam.Name} - {game.StartDateLeagueTime:MMMM d, yyyy}";
            string expectedTitleB = $"NHL Highlights | {game.AwayTeam.Name} vs. {game.HomeTeam.Name} - {game.StartDateLeagueTime:MMMM d, yyyy}";

            foreach (YouTubeVideo video in highlightPlaylist.Videos)
            {
                bool titleDateMatch1 = video.Title.Contains(game.StartDateLeagueTime.ToString("MMMM d, yyyy"));
                bool titleDateMatch2 = video.Title.Contains(game.StartDateLeagueTime.ToString("MMM d, yyyy"));
                bool titleDateMatch = titleDateMatch1 || titleDateMatch2;
                if (!titleDateMatch)
                    continue;

                // Allow highlight from game day and the next day
                bool publishDateMatch = video.PublishedDateTimeLeague >= game.StartDateLeagueTime && video.PublishedDateTimeLeague.AddDays(1) <= game.StartDateLeagueTime;
                if (!titleDateMatch)
                    continue;

                int percentageMatchA = Fuzz.Ratio(expectedTitleA, video.Title);
                int percentageMatchB = Fuzz.Ratio(expectedTitleB, video.Title);
                bool titleMatch = percentageMatchA >= requiredVideoTitleMatchPercentage || percentageMatchB >= requiredVideoTitleMatchPercentage;
                if (titleMatch)
                {
                    saveChanges = true;

                    game.YouTubeLink = video.Link;
                    break;
                }
            }
        }

        if (saveChanges)
            _dbContext.SaveChangesAsync();

        return highlightPlaylist;
    }*/

    /*public static YouTubePlaylist MatchVideosToGames(
        Schedule schedule,
        YouTubePlaylist highlightPlaylist,
        PlaylistConfiguration config)
    {
        Game[] games = schedule.GameDays
            .SelectMany(x => x.Games)
            .ToArray();

        foreach (Game game in games)
        {
            if (!string.IsNullOrEmpty(game.YouTubeLink))
                continue;

            // TODO: Remove .Where(x => x.Title.Contains())...
            YouTubeVideo[] highlightVideos = highlightPlaylist.Videos.Where(x => x.Title.Contains(game.HomeTeam.Name) || x.Title.Contains(game.AwayTeam.Name)).ToArray();
            foreach (YouTubeVideo video in highlightVideos)
            {
                int reasonablePublishDate = (video.PublishedDateTimeLeague.Date >= game.StartDateLeagueTime.Date && video.PublishedDateTimeLeague.Date < game.StartDateLeagueTime.AddDays(2).Date) ? 1 : 0;

                // TODO: Replace {GameNumber}. ie. NHL Game {GameNumber} Highlights.
                int titleMatch = config.TitleIdentifiers.Any(ti =>
                {
                    //video.Title.Contains
                    //string newTitleIdentifier1 = ti.Replace("{GameNumber}", @"\d");
                    //bool match = video.Title.Contains(newTitleIdentifier1);
                    //return match;

                    // Escape the pattern to treat it as a literal string
                    string pattern;
                    if (ti.Contains("{GameNumber}"))
                    {
                        pattern = Regex.Escape(ti);
                        pattern = pattern.Replace(Regex.Escape("{GameNumber}"), @"\d{1}");
                    }
                    else
                    {
                        bool match2 = video.Title.Contains(ti);
                        return match2;
                    }

                    // Use Regex.IsMatch with IgnoreCase for robustness
                    bool match = Regex.IsMatch(video.Title, pattern, RegexOptions.IgnoreCase);
                    return match;
                }) ? 1 : 0;

                // TODO: This doesn't account for spelling mistakes or minor alterations. ie. BC Lions vs. B.C. Lions.
                int teamsMatch = config.TeamFormats.Any(tf =>
                {
                    string newTeamFormat1 = tf.Replace("{TeamNameA}", game.HomeTeam.Name)
                        .Replace("{TeamNameB}", game.AwayTeam.Name)
                        .Replace("{TeamCityA}", game.HomeTeam.City)
                        .Replace("{TeamCityB}", game.AwayTeam.City);
                    string newTeamFormat2 = tf.Replace("{TeamNameA}", game.AwayTeam.Name)
                        .Replace("{TeamNameB}", game.HomeTeam.Name)
                        .Replace("{TeamCityA}", game.AwayTeam.City)
                        .Replace("{TeamCityB}", game.HomeTeam.City);

                    bool match = video.Title.Contains(newTeamFormat1) || video.Title.Contains(newTeamFormat2);
                    return match;
                }) ? 1 : 0;

                // TODO: Replace {WeekNumber}. ie. CFL Week {WeekNumber}.
                int dateMatch = config.DateFormats.Any(df =>
                {
                    //string newDateFormat1 = df.Replace("{WeekNumber}", @"\d\d");

                    //bool match = video.Title.Contains(game.StartDateUtc.ToString(df));
                    //return match;

                    //if (string.IsNullOrEmpty(df)) // Skip the "" (no date) format
                    //    return false;

                    string pattern;
                    if (df.Contains("{WeekNumber}"))
                    {
                        // It's a placeholder format (e.g., "CFL Week {WeekNumber}")
                        pattern = Regex.Escape(df);
                        pattern = pattern.Replace(Regex.Escape("{WeekNumber}"), @"\d{1,2}"); // \d{1,2} = 1 or 2 digits
                    }
                    else
                    {
                        // It's a standard DateTime format (e.g., "MMMM d, yyyy" or "(M/d/yy)")
                        string dateString = game.StartDateLeagueTime.ToString(df);
                        pattern = Regex.Escape(dateString); // Escape it to handle '(', ')', etc.
                    }

                    bool match = Regex.IsMatch(video.Title, pattern, RegexOptions.IgnoreCase);
                    return match;
                }) ? 1 : 0;


                int matchCount = reasonablePublishDate + titleMatch + teamsMatch + dateMatch;

                var video1 = video;
                var game1 = game;
                var home1 = game.HomeTeam;
                var away1 = game.AwayTeam;
                if (matchCount >= config.RequiredVideoPartMatches)
                {
                    game.YouTubeLink = video.Link;
                    break;
                }
            }
        }

        return highlightPlaylist;
    }*/

    /// <summary>
    /// Get 10 latest YouTube videos from playlist.
    /// </summary>
    private static YouTubePlaylist FetchLatestVideosFromRssFeed(string playlistId, Leagues league)
    {
        string youtubeRssUrl = $"https://www.youtube.com/feeds/videos.xml?playlist_id={playlistId}";

        using XmlReader reader = XmlReader.Create(youtubeRssUrl);

        SyndicationFeed feed = SyndicationFeed.Load(reader);

        YouTubePlaylist playlist = new()
        {
            Id = playlistId,
            Name = feed.Title.Text,
            ChannelName = feed.Authors.First().Name,
            LeagueId = league.Value,
            Videos = feed.Items.Select(video => new YouTubeVideo(
                video.Id.Split(":").Last(),
                video.Title.Text,
                video.PublishDate.DateTime,
                video.PublishDate.DateTime.ConvertToLeagueDateTime(league),
                video.Links.FirstOrDefault()?.Uri.ToString() ?? string.Empty)
            )
            .ToList()
        };

        return playlist;
    }

    private List<PlaylistConfiguration> GetPlaylistConfigurations(Leagues league)
    {
        if (!_youtubeConfigOptions.Value.LeaguePlaylists.TryGetValue(league.Name, out LeaguePlaylistSettings? playlistSettings) || playlistSettings is null)
            throw new Exception($"No properly formatted playlist settings found for league '{league.Name}'.");

        List<PlaylistConfiguration> playlistConfigs = new();
        string playlistSelectionMode = playlistSettings.SelectPlaylist;
        switch (playlistSelectionMode)
        {
            case "All":
                playlistConfigs.AddRange(playlistSettings.Playlists);
                break;
            case "Single":
            case "First":
                playlistConfigs.Add(playlistSettings.Playlists.First());
                break;
            default:
                throw new NotImplementedException($"Unrecognized or missing playlist selection mode for league '{league.Name}'.");
        }

        return playlistConfigs;
    }

    /// <summary>
    /// Check to see if we need to check for new videos. ie. Are any games even missing highlights?
    /// Using estimated end time for a game to try to calculate when we might want to check even more frequently?
    /// </summary>
    private int CalculateCheckInterval()
    {
        return 0;
    }
}
