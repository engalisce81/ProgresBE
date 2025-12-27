using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dev.Acadmy.Migrations
{
    /// <inheritdoc />
    public partial class URLMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "AppLecturesApp");

            migrationBuilder.DropColumn(
                name: "IntroductionVideoUrl",
                table: "AppCoursesProgres");

            migrationBuilder.DropColumn(
                name: "TargetUrl",
                table: "AppAdvertisementsProgres");

            migrationBuilder.AddColumn<string>(
                name: "DriveVideoUrl",
                table: "AppLecturesApp",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequiredQuiz",
                table: "AppLecturesApp",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "YouTubeVideoUrl",
                table: "AppLecturesApp",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriveVideoUrl",
                table: "AppCoursesProgres",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YouTubeVideoUrl",
                table: "AppCoursesProgres",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriveVideoUrl",
                table: "AppAdvertisementsProgres",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YouTubeVideoUrl",
                table: "AppAdvertisementsProgres",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriveVideoUrl",
                table: "AppLecturesApp");

            migrationBuilder.DropColumn(
                name: "IsRequiredQuiz",
                table: "AppLecturesApp");

            migrationBuilder.DropColumn(
                name: "YouTubeVideoUrl",
                table: "AppLecturesApp");

            migrationBuilder.DropColumn(
                name: "DriveVideoUrl",
                table: "AppCoursesProgres");

            migrationBuilder.DropColumn(
                name: "YouTubeVideoUrl",
                table: "AppCoursesProgres");

            migrationBuilder.DropColumn(
                name: "DriveVideoUrl",
                table: "AppAdvertisementsProgres");

            migrationBuilder.DropColumn(
                name: "YouTubeVideoUrl",
                table: "AppAdvertisementsProgres");

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "AppLecturesApp",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IntroductionVideoUrl",
                table: "AppCoursesProgres",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetUrl",
                table: "AppAdvertisementsProgres",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
