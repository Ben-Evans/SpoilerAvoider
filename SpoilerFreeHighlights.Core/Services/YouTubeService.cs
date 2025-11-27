using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;

namespace SpoilerFreeHighlights.Core.Services;

public class YouTubeService(AppDbContext _dbContext, IConfiguration _configuration)
{
    private static readonly ILogger _logger = Log.ForContext<YouTubeService>();

    /// <summary>
    /// For each league x league playlist:
    /// - Check for new videos and save references for ones that might be highlights.
    /// - Set game links if ready.
    /// </summary>
    /// <returns>True if new videos were saved.</returns>
    public async Task<bool> FetchAndCacheNewVideos()
    {
        bool strictVideoCaching = _configuration.GetValue("StrictVideoCaching", false);

        LeagueConfiguration[] leagueConfigurations = await _dbContext.LeagueConfigurations.ToArrayAsync();

        bool newVideosAdded = false;
        foreach (Leagues league in Leagues.GetAllLeagues())
        {
            List<PlaylistConfiguration> playlistConfigs = GetPlaylistConfigurations(leagueConfigurations, league);
            foreach (PlaylistConfiguration playlistConfig in playlistConfigs)
            {
                YouTubePlaylist newPlaylist = FetchLatestVideosFromRssFeed(playlistConfig.PlaylistId, playlistConfig.ChannelId, league);

                DateTime daysForward = league.LeagueDateTimeNow.AddDays(_configuration.GetValue<int>("FetchDaysForward")).Date;
                DateTime daysBack = league.LeagueDateTimeNow.AddDays(-_configuration.GetValue<int>("FetchDaysBack")).Date;

                // Discard any shorts or videos with unlikely dates
                newPlaylist.Videos = newPlaylist.Videos
                    .Where(x => !x.Link.Contains("/shorts/") && x.PublishedDateTimeLeague.Date >= daysBack && x.PublishedDateTimeLeague.Date < daysForward)
                    .ToList();

                if (strictVideoCaching)
                    newPlaylist.Videos = newPlaylist.Videos.Where(x => CheckIsHighlightAndSetAdditionalVideoInfo(x, playlistConfig)).ToList();

                if (!newPlaylist.Videos.Any())
                    continue;

                // Extract video information from title for highlight videos. Could filter by this instead, but then mislabelled or mismatched videos would be discarded too.
                if (!strictVideoCaching)
                    newPlaylist.Videos.ForEach(x => CheckIsHighlightAndSetAdditionalVideoInfo(x, playlistConfig));

                YouTubePlaylist? existingPlaylist = await _dbContext.YouTubePlaylists
                    .AsTracking()
                    .Include(x => x.Videos)
                    .FirstOrDefaultAsync(x => x.PlaylistId == playlistConfig.PlaylistId || x.ChannelId == playlistConfig.ChannelId);

                if (existingPlaylist is null)
                {
                    _dbContext.YouTubePlaylists.Add(newPlaylist);
                    await _dbContext.SaveChangesAsync();

                    newVideosAdded = true;
                }
                else
                {
                    YouTubeVideo[] newVideosToAdd = existingPlaylist.Videos.GetNewItems(newPlaylist.Videos, x => x.Id);
                    if (newVideosToAdd.Any())
                    {
                        existingPlaylist.Videos.AddRange(newVideosToAdd);
                        await _dbContext.SaveChangesAsync();

                        newVideosAdded = true;
                    }
                }
            }
        }
        return newVideosAdded;
    }

    /// <summary>
    /// Get 15 latest YouTube videos from playlist.
    /// </summary>
    public static YouTubePlaylist FetchLatestVideosFromRssFeed(string playlistId, string channelId, Leagues league)
    {
        return !string.IsNullOrEmpty(playlistId)
            ? FetchLatestVideosFromRssFeedByPlaylist(playlistId, league)
            : FetchLatestVideosFromRssFeedByChannel(channelId, league);
    }

    /// <summary>
    /// <inheritdoc cref="FetchLatestVideosFromRssFeed"/>
    /// </summary>
    private static SyndicationFeed FetchLatestVideosFromRssFeedByRssArg(string rssArgument)
    {
        string youtubeRssUrl = $"https://www.youtube.com/feeds/videos.xml?{rssArgument}";

        try
        {
            using XmlReader reader = XmlReader.Create(youtubeRssUrl);

            SyndicationFeed feed = SyndicationFeed.Load(reader);
            return feed;
        }
        catch (Exception)
        {
            _logger.Error("Failed to fetch YouTube RSS data from url: '{URL}'.", youtubeRssUrl);
            throw;
        }
    }

    /// <summary>
    /// <inheritdoc cref="FetchLatestVideosFromRssFeed"/>
    /// </summary>
    private static YouTubePlaylist FetchLatestVideosFromRssFeedByPlaylist(string playlistId, Leagues league)
    {
        if (string.IsNullOrEmpty(playlistId))
            throw new ArgumentException($"{nameof(playlistId)} must be provided.");

        SyndicationFeed feed = FetchLatestVideosFromRssFeedByRssArg($"playlist_id={playlistId}");

        string channelId = feed.ElementExtensions
                .ReadElementExtensions<string>("channelId", "http://www.youtube.com/xml/schemas/2015")
                .FirstOrDefault() ?? string.Empty;

        YouTubePlaylist playlist = new(playlistId, feed.Title.Text, channelId, feed.Authors.First().Name)
        {
            //Id = string.Empty,
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

    /// <summary>
    /// <inheritdoc cref="FetchLatestVideosFromRssFeed"/>
    /// </summary>
    private static YouTubePlaylist FetchLatestVideosFromRssFeedByChannel(string channelId, Leagues league)
    {
        if (string.IsNullOrEmpty(channelId))
            throw new ArgumentException($"{nameof(channelId)} must be provided.");

        SyndicationFeed feed = FetchLatestVideosFromRssFeedByRssArg($"channel_id={channelId}");

        YouTubePlaylist playlist = new(string.Empty, string.Empty, channelId, feed.Title.Text)
        {
            //Id = string.Empty,
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

    /// <summary>
    /// For each league x league playlist:
    /// - Set game links if ready.
    /// </summary>
    public async Task AddYouTubeLinksToAllMatches()
    {
        foreach (Leagues league in Leagues.GetAllLeagues())
            await AddYouTubeLinksToMatches(league);
    }

    /// <summary>
    /// <inheritdoc cref="AddYouTubeLinksToAllMatches" />
    /// </summary>
    private async Task AddYouTubeLinksToMatches(Leagues league)
    {
        LeagueConfiguration[] leagueConfigurations = await _dbContext.LeagueConfigurations.ToArrayAsync();

        List<PlaylistConfiguration> playlistConfigs = GetPlaylistConfigurations(leagueConfigurations, league);
        if (!playlistConfigs.Any())
            return;

        bool usePlaylistIds = playlistConfigs.All(x => !string.IsNullOrEmpty(x.PlaylistId));
        string[] playlistIds = playlistConfigs.Select(x => x.PlaylistId).Where(x => !string.IsNullOrEmpty(x)).ToArray();
        string[] channelIds = playlistConfigs.Select(x => x.ChannelId).Where(x => !string.IsNullOrEmpty(x)).ToArray();

        // Filter videos by ones with Extracted fields populated to get highlight videos.
        IQueryable<YouTubePlaylist> playlistQuery = _dbContext.YouTubePlaylists
            .Include(x => x.Videos.Where(y => !string.IsNullOrEmpty(y.ExtractedTitleTeamA) && !string.IsNullOrEmpty(y.ExtractedTitleTeamB)).OrderBy(y => y.PublishedDateUtc))
            .Where(x => x.LeagueId == league.Value);
        if (usePlaylistIds)
            playlistQuery = playlistQuery.Where(x => playlistIds.Contains(x.PlaylistId));
        else
            playlistQuery = playlistQuery.Where(x => channelIds.Contains(x.ChannelId));

        YouTubePlaylist[] playlists = await playlistQuery.ToArrayAsync();
        YouTubeVideo[] videos = playlists.SelectMany(x => x.Videos)
            .OrderBy(x => x.PublishedDateTimeLeague)
            .ToArray();

        //DateTime daysBack = league.LeagueDateTimeNow.AddDays(-_configuration.GetValue<int>("FetchDaysBack")).Date;
        //if (videos.All(x => x.PublishedDateTimeLeague.Date >= daysBack))
        if (!videos.Any())
            return;

        DateTime leagueTomorrow = league.LeagueDateTimeToday.AddDays(1);
        DateTime minDateTimeLeague = videos.First().PublishedDateTimeLeague.Date;
        DateTime maxDateTimeLeague = videos.Last().PublishedDateTimeLeague.AddDays(1).Date;
        maxDateTimeLeague = leagueTomorrow > maxDateTimeLeague ? leagueTomorrow : maxDateTimeLeague;

        Game[] games = await _dbContext.Games
            .AsTracking()
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .Where(x => x.LeagueId == league.Value && string.IsNullOrEmpty(x.YouTubeLink)
                && x.StartDateLeagueTime.Date >= minDateTimeLeague && x.StartDateLeagueTime.Date <= maxDateTimeLeague)
            .OrderBy(x => x.StartDateUtc)
            .ToArrayAsync();

        if (!games.Any())
            return;

        bool saveChanges = false;

        foreach (YouTubePlaylist playlist in playlists)
        {
            PlaylistConfiguration playlistConfig = playlistConfigs.First(x => x.PlaylistId == playlist.PlaylistId || x.ChannelId == playlist.ChannelId);

            foreach (Game game in games)
            {
                DateOnly gameDate = DateOnly.FromDateTime(game.StartDateLeagueTime);

                List<YouTubeVideo> gameVideos = playlist.Videos
                    .Where(x =>
                        (game.HomeTeam.ToString().Contains(x.ExtractedTitleTeamA) || game.AwayTeam.ToString().Contains(x.ExtractedTitleTeamA))
                        && (game.HomeTeam.ToString().Contains(x.ExtractedTitleTeamB) || game.AwayTeam.ToString().Contains(x.ExtractedTitleTeamB)))
                    .ToList();
                if (!gameVideos.Any())
                    continue;

                // Allow highlight from game day and the next day. Might be an issue for a back to back.
                gameVideos = gameVideos.Where(x =>
                    //(x.ExtractedTitleDate.Value == gameDate || x.ExtractedTitleDate.Value == gameDate.AddDays(1))
                    (x.PublishedDateTimeLeague.Date == game.StartDateLeagueTime.Date || x.PublishedDateTimeLeague.Date == game.StartDateLeagueTime.Date.AddDays(1))).ToList();

                YouTubeVideo? gameVideo = gameVideos.FirstOrDefault();
                if (gameVideo is not null)
                {
                    _logger.Debug("Game: {GameInfo} matches {VideoInfo}.", game.ToString(), gameVideo.ToString());
                    saveChanges = true;

                    game.YouTubeLink = gameVideo.Link;
                }
            }
        }

        if (saveChanges)
            await _dbContext.SaveChangesAsync();
    }

    private static List<PlaylistConfiguration> GetPlaylistConfigurations(LeagueConfiguration[] leagueConfigs, Leagues league)
    {
        LeagueConfiguration? leagueConfig = leagueConfigs.FirstOrDefault(x => x.LeagueId == league.Value);
        if (leagueConfig is null || !leagueConfig.Playlists.Any())
            //throw new Exception($"No league configuration found for league '{league.Name}'.");
            return [];

        List<PlaylistConfiguration> playlistConfigs = [];
        string playlistSelectionMode = leagueConfig.SelectPlaylistType;
        switch (playlistSelectionMode)
        {
            case "All":
                playlistConfigs.AddRange(leagueConfig.Playlists);
                break;
            // TODO: Need to add an order property to the playlist model to ensure this works properly
            case "Single":
            case "First":
                playlistConfigs.Add(leagueConfig.Playlists.First());
                break;
            default:
                throw new NotImplementedException($"Unrecognized or missing playlist selection mode for league '{league.DisplayName}'.");
        }

        return playlistConfigs;
    }

    /// <summary>
    /// If video matches playlist's video title pattern then set the extracted title values.
    /// </summary>
    /// <returns>Is a highlight video.</returns>
    private static bool CheckIsHighlightAndSetAdditionalVideoInfo(YouTubeVideo video, PlaylistConfiguration playlistConfig)
    {
        string highlightPattern = playlistConfig.TitlePattern;
        Match highlightMatch = Regex.Match(video.Title, highlightPattern);
        if (!highlightMatch.Success)
            return false;

        if (highlightMatch.Groups.TryGetValue("teamA", out Group? teamNameAGroup))
            video.ExtractedTitleTeamA = teamNameAGroup.Value;

        if (highlightMatch.Groups.TryGetValue("teamB", out Group? teamNameBGroup))
            video.ExtractedTitleTeamB = teamNameBGroup.Value;

        if (highlightMatch.Groups.TryGetValue("date", out Group? dateGroup)
            && DateOnly.TryParseExact(dateGroup.Value, playlistConfig.DateFormats.Select(x => x.Value).ToArray(), out DateOnly extractedDate))
            video.ExtractedTitleDate = extractedDate;

        if (highlightMatch.Groups.TryGetValue("identA", out Group? identifierAGroup))
            video.ExtractedTitleIdentifierA = identifierAGroup.Value;

        if (highlightMatch.Groups.TryGetValue("identB", out Group? identifierBGroup))
            video.ExtractedTitleIdentifierB = identifierBGroup.Value;

        return true;
    }
}
