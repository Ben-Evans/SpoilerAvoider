using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpoilerFreeHighlights.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingPlaylists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YouTubeVideos_YouTubePlaylists_PlaylistId",
                table: "YouTubeVideos");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoTitleTeamFormats_PlaylistConfigurations_PlaylistConfigurationId",
                table: "VideoTitleTeamFormats");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoTitleIdentifiers_PlaylistConfigurations_PlaylistConfigurationId",
                table: "VideoTitleIdentifiers");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoTitleDateFormats_PlaylistConfigurations_PlaylistConfigurationId",
                table: "VideoTitleDateFormats");

            migrationBuilder.DropIndex(
                name: "IX_YouTubeVideos_PlaylistId",
                table: "YouTubeVideos");

            migrationBuilder.DropIndex(
                name: "IX_VideoTitleTeamFormats_PlaylistConfigurationId",
                table: "VideoTitleTeamFormats");

            migrationBuilder.DropIndex(
                name: "IX_VideoTitleIdentifiers_PlaylistConfigurationId",
                table: "VideoTitleIdentifiers");

            migrationBuilder.DropIndex(
                name: "IX_VideoTitleDateFormats_PlaylistConfigurationId",
                table: "VideoTitleDateFormats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YouTubePlaylists",
                table: "YouTubePlaylists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlaylistConfigurations",
                table: "PlaylistConfigurations");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "YouTubePlaylists");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PlaylistConfigurations");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "YouTubeVideos");

            migrationBuilder.DropColumn(
                name: "PlaylistConfigurationId",
                table: "VideoTitleTeamFormats");

            migrationBuilder.DropColumn(
                name: "PlaylistConfigurationId",
                table: "VideoTitleIdentifiers");

            migrationBuilder.DropColumn(
                name: "PlaylistConfigurationId",
                table: "VideoTitleDateFormats");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "YouTubePlaylists",
                newName: "PlaylistName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "PlaylistConfigurations",
                newName: "PlaylistName");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "YouTubePlaylists",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "ChannelId",
                table: "YouTubePlaylists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlaylistId",
                table: "YouTubePlaylists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PlaylistId",
                table: "YouTubeVideos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PlaylistConfigurations",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "ChannelId",
                table: "PlaylistConfigurations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "PlaylistConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PlaylistId",
                table: "PlaylistConfigurations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PlaylistConfigurationId",
                table: "VideoTitleTeamFormats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaylistConfigurationId",
                table: "VideoTitleIdentifiers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaylistConfigurationId",
                table: "VideoTitleDateFormats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_YouTubePlaylists",
                table: "YouTubePlaylists",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaylistConfigurations",
                table: "PlaylistConfigurations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_YouTubeVideos_PlaylistId",
                table: "YouTubeVideos",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleTeamFormats_PlaylistConfigurationId",
                table: "VideoTitleTeamFormats",
                column: "PlaylistConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleIdentifiers_PlaylistConfigurationId",
                table: "VideoTitleIdentifiers",
                column: "PlaylistConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleDateFormats_PlaylistConfigurationId",
                table: "VideoTitleDateFormats",
                column: "PlaylistConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_YouTubeVideos_YouTubePlaylists_PlaylistId",
                table: "YouTubeVideos",
                column: "PlaylistId",
                principalTable: "YouTubePlaylists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoTitleTeamFormats_PlaylistConfigurations_PlaylistConfigurationId",
                table: "VideoTitleTeamFormats",
                column: "PlaylistConfigurationId",
                principalTable: "PlaylistConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoTitleIdentifiers_PlaylistConfigurations_PlaylistConfigurationId",
                table: "VideoTitleIdentifiers",
                column: "PlaylistConfigurationId",
                principalTable: "PlaylistConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoTitleDateFormats_PlaylistConfigurations_PlaylistConfigurationId",
                table: "VideoTitleDateFormats",
                column: "PlaylistConfigurationId",
                principalTable: "PlaylistConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YouTubeVideos_YouTubePlaylists_PlaylistId",
                table: "YouTubeVideos");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoTitleTeamFormats_PlaylistConfigurations_PlaylistConfigurationId",
                table: "VideoTitleTeamFormats");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoTitleIdentifiers_PlaylistConfigurations_PlaylistConfigurationId",
                table: "VideoTitleIdentifiers");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoTitleDateFormats_PlaylistConfigurations_PlaylistConfigurationId",
                table: "VideoTitleDateFormats");

            migrationBuilder.DropIndex(
                name: "IX_YouTubeVideos_PlaylistId",
                table: "YouTubeVideos");

            migrationBuilder.DropIndex(
                name: "IX_VideoTitleTeamFormats_PlaylistConfigurationId",
                table: "VideoTitleTeamFormats");

            migrationBuilder.DropIndex(
                name: "IX_VideoTitleIdentifiers_PlaylistConfigurationId",
                table: "VideoTitleIdentifiers");

            migrationBuilder.DropIndex(
                name: "IX_VideoTitleDateFormats_PlaylistConfigurationId",
                table: "VideoTitleDateFormats");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "YouTubePlaylists");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "YouTubePlaylists");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "PlaylistConfigurations");

            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "PlaylistConfigurations");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "PlaylistConfigurations");

            migrationBuilder.RenameColumn(
                name: "PlaylistName",
                table: "YouTubePlaylists",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "PlaylistName",
                table: "PlaylistConfigurations",
                newName: "Name");

            migrationBuilder.AlterColumn<string>(
                name: "PlaylistId",
                table: "YouTubeVideos",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "YouTubePlaylists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "PlaylistConfigurationId",
                table: "VideoTitleTeamFormats",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PlaylistConfigurationId",
                table: "VideoTitleIdentifiers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PlaylistConfigurationId",
                table: "VideoTitleDateFormats",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "PlaylistConfigurations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YouTubePlaylists",
                table: "YouTubePlaylists",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaylistConfigurations",
                table: "PlaylistConfigurations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_YouTubeVideos_PlaylistId",
                table: "YouTubeVideos",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleTeamFormats_PlaylistConfigurationId",
                table: "VideoTitleTeamFormats",
                column: "PlaylistConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleIdentifiers_PlaylistConfigurationId",
                table: "VideoTitleIdentifiers",
                column: "PlaylistConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTitleDateFormats_PlaylistConfigurationId",
                table: "VideoTitleDateFormats",
                column: "PlaylistConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_YouTubeVideos_YouTubePlaylists_PlaylistId",
                table: "YouTubeVideos",
                column: "PlaylistId",
                principalTable: "YouTubePlaylists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoTitleTeamFormats_PlaylistConfigurations_PlaylistConfigurationId",
                table: "VideoTitleTeamFormats",
                column: "PlaylistConfigurationId",
                principalTable: "PlaylistConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoTitleIdentifiers_PlaylistConfigurations_PlaylistConfigurationId",
                table: "VideoTitleIdentifiers",
                column: "PlaylistConfigurationId",
                principalTable: "PlaylistConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoTitleDateFormats_PlaylistConfigurations_PlaylistConfigurationId",
                table: "VideoTitleDateFormats",
                column: "PlaylistConfigurationId",
                principalTable: "PlaylistConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
