# Live Site
Available [here](https://jolly-sand-0577cdc10.3.azurestaticapps.net).

# Setup Instructions
Based on fluent playlist needs it will likely make sense to switch to channel only or have dynamic playlists found automatically in the future.

### NHL
Playlist contains both regular season only, requires additional playoff playlist. Requires new playlists yearly. SelectPlaylist = All. Playlist 1 stopped getting uploads, so just use Channel Id if that's the case.

Playlist #1. (Playlist Name: `2025-26 NHL Highlights, News and Analysis`, ChannelName: `sportsnet`)
- Channel Id: `UCVhibwHk4WKw4leUt6JfRLg`
- Playlist Id: `PLo12SYwt93SQP81ntu8rz9QcAHTNQaFI3`
- Title Pattern: `^(?<identA>NHL (?:|Game )Highlights) \| (?<teamA>.+?) vs\. (?<teamB>.+?) - (?<date>\w+ \d{1,2}, \d{4})$`
- Title Identifiers: `NHL Highlights`, `NHL Game Highlights`
- Team Formats: `{TeamNameA} vs. {TeamNameB}`
- Date Formats: `MMMM d, yyyy`, `MMM d, yyyy`

Playlist #2. (Playlist Name: `2025 Stanley Cup Playoffs | NHL Highlights, News and Interviews`, ChannelName: `sportsnet`)
- Playlist Id: `PLo12SYwt93SSAClobQd6Pmps7-b6sW9Z1`
- Title Pattern: `^(?<identA>(?:NHL (?:|Game )|Stanley Cup Final Game \d )Highlights) \| (?<teamA>.+?) vs\. (?<teamB>.+?) - (?<date>\w+ \d{1,2}, \d{4})$`
- Title Identifiers: `NHL Highlights`, `NHL Game Highlights`, `Game {GameNumber} Highlights`
- Team Formats: *same as playlist 1*
- Date Formats: *same as playlist 1*

### MLB
Playlist contains both regular season and playoffs. Requires new playlist yearly. SelectPlaylist = Single.

Playlist #1. (Playlist Name: `2025 Game Highlights`, ChannelName: `MLB`)
- Playlist Id: `PLL-lmlkrmJanq-c41voXY4cCbxVR0bjxR`
- Title Pattern: `^(?<teamA>.+) vs\. (?<teamB>.+?) (?<identA>(?:(?:(?:AL|NL)(?:CS|DS)|(?:NL Division|World) Series|(?:AL|NL) Wild Card) )?Game(?:| \d) Highlights) \((?<date>\d{1,2}\/\d{1,2}\/\d{1,2})\) \| (?<identB>MLB Highlights)$`
- Title Identifiers: `MLB Highlights`
- Team Formats: `{TeamNameA} vs. {TeamNameB}`
- Date Formats: `M/d/yy`

### CFL
Playlist 1 shouldn't need to be updated since it contains all seasons, while playlist 2 would need to be updated. SelectPlaylist = First / don't bother with playlist 2. Title Pattern could be revamped, but the inconsistency makes it extremely difficult.

Playlist #1. (Playlist Name: `CFL`, ChannelName: `TSN`)
- Playlist Id: `PL6ndzuHRRemBD4OSSinVDddyjw_W0-My6`
- Title Pattern: `^(?<identA>CFL (?:WEEK \d+(?:: | HIGHLIGHTS: )|(?:WESTERN|EASTERN|WEST|EAST) SEMI-FINAL: |(?:West|East) Final: )|(?:\d+(?:st|nd|rd|th) Grey Cup ))?(?<teamA>.+?) vs\. (?<teamB>.+?)(?:\s+\|\s+(?<identB>FULL HIGHLIGHTS|\d+(?:st|nd|rd|th) Grey Cup Highlights)|\s+(?<identC>FULL HIGHLIGHTS))$`
- Title Identifiers: `FULL HIGHLIGHTS`, `Grey Cup Highlights`, `CFL West Final`, `CFL East Final`, `WEST SEMI-FINAL`, `EAST SEMI-FINAL`
- Team Formats: `{TeamCityA} {TeamNameA} vs. {TeamCityB} {TeamNameB}`
- ~~Date Formats: ``~~

Playlist #2 (Playlist Name: `CFL Highlights`, ChannelName: `TSN`)
- Playlist Id: `PL6ndzuHRRemADTppEA6X_uAhgyiU_2ORj`
- Title Pattern: *same as playlist 1*
- Title Identifiers: *same as playlist 1*
- Team Formats: *same as playlist 1*
- ~~Date Formats: ``~~

# Example YouTube Video Titles

| League | Title                                                                                        | Is Highlight |
| ------ | -------------------------------------------------------------------------------------------- | ------------ |
| NHL    | NHL Highlights \| Devils vs. Maple Leafs - October 21, 2025                                  | Yes          |
| NHL    | NHL Game Highlights \| Penguins vs. Sharks - Oct 18, 2025                                    | Yes          |
| NHL    | Stanley Cup Final Game 6 Highlights \| Oilers vs. Panthers - June 17, 2025                   | Yes          |
| NHL    | NHL Game 3 Highlights \| Oilers vs. Panthers - June 9, 2025                                  | Yes          |
| NHL    | Stanley Cup Final Game 1 Highlights \| Panthers vs. Oilers - June 4, 2025                    | Yes          |
| NHL    | NHL Highlights: Predators 6, Red Wings 3                                                     | Yes          |
| NHL    | Ottawa Senators at Vegas Golden Knights \| FULL Shootout Highlights - November 26, 2025      | No           |
| NHL    | Canucks' Garland dekes out Ducks defenders to score spectacular goal                         | No           |
| NHL    | Maple Leafs' William Nylander Fires In OT Winner With Slick Wrister vs. Blue Jackets         | No           |
| NHL    | Toronto Maple Leafs at Columbus Blue Jackets \| FULL Overtime Highlights - November 26, 2025 | No           |
| MLB    | Rays vs. Blue Jays Game Highlights (9/28/25) \| MLB Highlights                               | Yes          |
| MLB    | Tigers vs. Guardians AL Wild Card Game 1 Highlights (9/30/25) \| MLB Highlights              | Yes          |
| MLB    | Padres vs. Cubs NL Wild Card Game 1 Highlights (9/30/25) \| MLB Highlights                   | Yes          |
| MLB    | Cubs vs. Brewers NLDS Game 1 Highlights (10/4/25) \| MLB Highlights                          | Yes          |
| MLB    | Yankees vs. Blue Jays ALDS Game 1 Highlights (10/4/25) \| MLB Highlights                     | Yes          |
| MLB    | Dodgers vs. Phillies NL Divisional Series Game 1 Highlights (10/4/25) \| MLB Highlights      | Yes          |
| MLB    | Mariners vs. Blue Jays ALCS Game 7 Highlights (10/20/25) \| MLB Highlights                   | Yes          |
| MLB    | Dodgers vs. Brewers NLCS Game 1 Highlights (10/13/25) \| MLB Highlights                      | Yes          |
| MLB    | Dodgers vs. Blue Jays World Series Game 7 Highlights (11/1/25) \| MLB Highlights             | Yes          |
| CFL    | CFL WESTERN SEMI-FINAL: Calgary Stampeders vs. BC Lions \| FULL HIGHLIGHTS                   | Yes          |
| CFL    | CFL EASTERN SEMI-FINAL: Montreal Alouettes vs. Winnipeg Blue Bombers \| FULL HIGHLIGHTS      | Yes          |
| CFL    | CFL WEEK 20: Toronto Argonauts vs. Calgary Stampeders \| FULL HIGHLIGHTS                     | Yes          |
| CFL    | CFL WEEK 19 HIGHLIGHTS: Ottawa Redblacks vs. Montreal Alouettes \| FULL HIGHLIGHTS           | Yes          |
| CFL    | CFL WEEK 1: Edmonton Elks vs. B.C. Lions \| FULL HIGHLIGHTS                                  | Yes          |
| CFL    | CFL WEEK 2: Winnipeg Blue Bombers vs. BC Lions \| FULL HIGHLIGHTS                            | Yes          |
| CFL    | Toronto Argonauts vs. Winnipeg Blue Bombers \| 111th Grey Cup Highlights                     | Yes          |
| CFL    | CFL West Final: Roughriders 22, Blue Bombers 38                                              | Yes          |
| CFL    | CFL East Final: Argonauts 30, Alouettes 28 \| FULL HIGHLIGHTS                                | Yes          |
| CFL    | WEST SEMI-FINAL: BC Lions vs. Saskatchewan Roughriders FULL HIGHLIGHTS                       | Yes          |
| CFL    | EAST SEMI-FINAL: Ottawa Redblacks vs. Toronto Argonauts FULL HIGHLIGHTS                      | Yes          |
| CFL    | CFL WEEK 21: Calgary Stampeders vs. Saskatchewan Roughriders FULL HIGHLIGHTS                 | Yes          |
