using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpoilerFreeHighlights.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeagueConfigurations",
                columns: table => new
                {
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    SelectPlaylistType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueConfigurations", x => x.LeagueId);
                    table.ForeignKey(
                        name: "FK_LeagueConfigurations_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    City = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LogoLink = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => new { x.LeagueId, x.Id });
                    table.ForeignKey(
                        name: "FK_Teams_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YouTubePlaylists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ChannelName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouTubePlaylists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YouTubePlaylists_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistConfigurations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ChannelName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RequiredVideoPartMatches = table.Column<int>(type: "int", nullable: false),
                    RequiredVideoTitlePercentageMatch = table.Column<int>(type: "int", nullable: false),
                    TitlePattern = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    LeagueConfigurationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistConfigurations_LeagueConfigurations_LeagueConfigurationId",
                        column: x => x.LeagueConfigurationId,
                        principalTable: "LeagueConfigurations",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    StartDateLeagueTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    YouTubeLink = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsHypothetical = table.Column<bool>(type: "bit", nullable: false),
                    HomeTeamId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AwayTeamId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => new { x.LeagueId, x.Id });
                    table.ForeignKey(
                        name: "FK_Games_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Games_Teams_LeagueId_AwayTeamId",
                        columns: x => new { x.LeagueId, x.AwayTeamId },
                        principalTable: "Teams",
                        principalColumns: new[] { "LeagueId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_Teams_LeagueId_HomeTeamId",
                        columns: x => new { x.LeagueId, x.HomeTeamId },
                        principalTable: "Teams",
                        principalColumns: new[] { "LeagueId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "YouTubeVideos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PublishedDateTimeLeague = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExtractedTitleTeamA = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExtractedTitleTeamB = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExtractedTitleDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExtractedTitleIdentifierA = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExtractedTitleIdentifierB = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PlaylistId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouTubeVideos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YouTubeVideos_YouTubePlaylists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "YouTubePlaylists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoTitleDateFormats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PlaylistConfigurationId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTitleDateFormats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoTitleDateFormats_PlaylistConfigurations_PlaylistConfigurationId",
                        column: x => x.PlaylistConfigurationId,
                        principalTable: "PlaylistConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoTitleIdentifiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PlaylistConfigurationId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTitleIdentifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoTitleIdentifiers_PlaylistConfigurations_PlaylistConfigurationId",
                        column: x => x.PlaylistConfigurationId,
                        principalTable: "PlaylistConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoTitleTeamFormats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PlaylistConfigurationId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTitleTeamFormats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoTitleTeamFormats_PlaylistConfigurations_PlaylistConfigurationId",
                        column: x => x.PlaylistConfigurationId,
                        principalTable: "PlaylistConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_LeagueId_AwayTeamId",
                table: "Games",
                columns: new[] { "LeagueId", "AwayTeamId" });

            migrationBuilder.CreateIndex(
                name: "IX_Games_LeagueId_HomeTeamId",
                table: "Games",
                columns: new[] { "LeagueId", "HomeTeamId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistConfigurations_LeagueConfigurationId",
                table: "PlaylistConfigurations",
                column: "LeagueConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleDateFormats_PlaylistConfigurationId",
                table: "VideoTitleDateFormats",
                column: "PlaylistConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleIdentifiers_PlaylistConfigurationId",
                table: "VideoTitleIdentifiers",
                column: "PlaylistConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleTeamFormats_PlaylistConfigurationId",
                table: "VideoTitleTeamFormats",
                column: "PlaylistConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_YouTubePlaylists_LeagueId",
                table: "YouTubePlaylists",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_YouTubeVideos_PlaylistId",
                table: "YouTubeVideos",
                column: "PlaylistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "VideoTitleDateFormats");

            migrationBuilder.DropTable(
                name: "VideoTitleIdentifiers");

            migrationBuilder.DropTable(
                name: "VideoTitleTeamFormats");

            migrationBuilder.DropTable(
                name: "YouTubeVideos");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "PlaylistConfigurations");

            migrationBuilder.DropTable(
                name: "YouTubePlaylists");

            migrationBuilder.DropTable(
                name: "LeagueConfigurations");

            migrationBuilder.DropTable(
                name: "Leagues");
        }
    }
}
