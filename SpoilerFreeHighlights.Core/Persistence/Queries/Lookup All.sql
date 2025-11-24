select * from Leagues

select l.Name AS 'League', t.Id, t.City, t.Name, t.Abbreviation, t.LogoLink
from Teams t
	join Leagues l ON l.Id = t.LeagueId
order by LeagueId, City

select l.Name AS 'League', g.Id, g.StartDateLeagueTime, th.Name AS 'Home Team', ta.Name AS 'Away Team', g.YouTubeLink
from Games g
	join Leagues l ON l.Id = g.LeagueId
	join Teams th ON th.Id = g.HomeTeamId
	join Teams ta ON ta.Id = g.AwayTeamId
order by g.LeagueId, StartDateUtc

select l.Name AS 'League', ytp.Id, ytp.Name, ytp.ChannelName
from YouTubePlaylists ytp
	join Leagues l ON l.Id = ytp.LeagueId
order by ytp.LeagueId

select l.Name AS 'League', ytp.Name AS 'Playlist', ytv.*
from YouTubeVideos ytv
	join YouTubePlaylists ytp ON ytp.Id = ytv.PlaylistId
	join Leagues l ON l.Id = ytp.LeagueId
order by ytp.LeagueId

select l.Name AS 'League', lc.SelectPlaylistType, pc.Id, pc.Name, pc.ChannelName, pc.TitlePattern, pc.Comment
from LeagueConfigurations lc
	join Leagues l ON l.Id = lc.LeagueId
	join PlaylistConfigurations pc ON pc.LeagueConfigurationId = lc.LeagueId
order by lc.LeagueId
