using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dev.Acadmy.Migrations
{
    /// <inheritdoc />
    public partial class StudentScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppLectureStudentsApp_AbpUsers_UserId",
                table: "AppLectureStudentsApp");

            migrationBuilder.DropForeignKey(
                name: "FK_AppQuizesApp_AppLecturesApp_LectureId",
                table: "AppQuizesApp");

            migrationBuilder.AddColumn<double>(
                name: "PassScore",
                table: "AppExamsProgres",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddForeignKey(
                name: "FK_AppLectureStudentsApp_AbpUsers_UserId",
                table: "AppLectureStudentsApp",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppQuizesApp_AppLecturesApp_LectureId",
                table: "AppQuizesApp",
                column: "LectureId",
                principalTable: "AppLecturesApp",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppLectureStudentsApp_AbpUsers_UserId",
                table: "AppLectureStudentsApp");

            migrationBuilder.DropForeignKey(
                name: "FK_AppQuizesApp_AppLecturesApp_LectureId",
                table: "AppQuizesApp");

            migrationBuilder.DropColumn(
                name: "PassScore",
                table: "AppExamsProgres");

            migrationBuilder.AddForeignKey(
                name: "FK_AppLectureStudentsApp_AbpUsers_UserId",
                table: "AppLectureStudentsApp",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppQuizesApp_AppLecturesApp_LectureId",
                table: "AppQuizesApp",
                column: "LectureId",
                principalTable: "AppLecturesApp",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
