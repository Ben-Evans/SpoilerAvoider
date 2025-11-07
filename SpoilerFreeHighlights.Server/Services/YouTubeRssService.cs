using Microsoft.Extensions.Options;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;

namespace SpoilerFreeHighlights.Server.Services;

public class YouTubeRssService(
    AppDbContext _dbContext,
    IConfiguration _configuration,
    IOptions<YouTubeSettings> _youtubeConfigOptions)
{
    private static readonly ILogger _logger = Log.ForContext<YouTubeRssService>();

    /// <summary>
    /// For each league x league playlist:
    /// - Check for new videos and save references for ones that might be highlights.
    /// - Set game links if ready.
    /// </summary>
    /// <returns>True if new videos were saved.</returns>
    public async Task<bool> FetchAndCacheNewVideos()
    {
        bool strictVideoCaching = _configuration.GetValue("StrictVideoCaching", false);

        bool newVideosAdded = false;
        foreach (Leagues league in Leagues.GetAllLeagues())
        {
            List<PlaylistConfiguration> playlistConfigs = GetPlaylistConfigurations(league);
            foreach (PlaylistConfiguration playlistConfig in playlistConfigs)
            {
                YouTubePlaylist newPlaylist = FetchLatestVideosFromRssFeed(playlistConfig.Id, league);

                DateTime daysForward = league.LeagueDateTimeNow.AddDays(_configuration.GetValue<int>("FetchDaysForwards")).Date;
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
                    .FirstOrDefaultAsync(x => x.Id == playlistConfig.Id);

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
        List<PlaylistConfiguration> playlistConfigs = GetPlaylistConfigurations(league);
        string[] playlistIds = playlistConfigs.Select(x => x.Id).ToArray();

        // Filter videos by ones with Extracted fields populated to get highlight videos.
        YouTubePlaylist[] playlists = await _dbContext.YouTubePlaylists
            .Include(x => x.Videos.Where(y => !string.IsNullOrEmpty(y.ExtractedTitleTeamA) && !string.IsNullOrEmpty(y.ExtractedTitleTeamB)).OrderBy(y => y.PublishedDateUtc))
            .Where(x => x.LeagueId == league.Value && playlistIds.Contains(x.Id))
            .ToArrayAsync();
        
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
            PlaylistConfiguration playlistConfig = playlistConfigs.First(x => x.Id == playlist.Id);

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

    /// <summary>
    /// Get 15 latest YouTube videos from playlist.
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

        if (highlightMatch.Groups.TryGetValue("date", out Group? dateGroup) && DateOnly.TryParseExact(dateGroup.Value, playlistConfig.DateFormats.ToArray(), out DateOnly extractedDate))
            video.ExtractedTitleDate = extractedDate;

        if (highlightMatch.Groups.TryGetValue("identA", out Group? identifierAGroup))
            video.ExtractedTitleIdentifierA = identifierAGroup.Value;

        if (highlightMatch.Groups.TryGetValue("identB", out Group? identifierBGroup))
            video.ExtractedTitleIdentifierB = identifierBGroup.Value;

        return true;
    }
}
