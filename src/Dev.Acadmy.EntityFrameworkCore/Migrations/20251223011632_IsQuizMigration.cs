using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dev.Acadmy.Migrations
{
    /// <inheritdoc />
    public partial class IsQuizMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "AppQuizesApp",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsQuiz",
                table: "AppCoursesProgres",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_AppQuizesApp_CourseId",
                table: "AppQuizesApp",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCoursesProgres_CreationTime",
                table: "AppCoursesProgres",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppCoursesProgres_IsQuiz_IsPdf",
                table: "AppCoursesProgres",
                columns: new[] { "IsQuiz", "IsPdf" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCoursesProgres_Name",
                table: "AppCoursesProgres",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_AppQuizesApp_AppCoursesProgres_CourseId",
                table: "AppQuizesApp",
                column: "CourseId",
                principalTable: "AppCoursesProgres",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppQuizesApp_AppCoursesProgres_CourseId",
                table: "AppQuizesApp");

            migrationBuilder.DropIndex(
                name: "IX_AppQuizesApp_CourseId",
                table: "AppQuizesApp");

            migrationBuilder.DropIndex(
                name: "IX_AppCoursesProgres_CreationTime",
                table: "AppCoursesProgres");

            migrationBuilder.DropIndex(
                name: "IX_AppCoursesProgres_IsQuiz_IsPdf",
                table: "AppCoursesProgres");

            migrationBuilder.DropIndex(
                name: "IX_AppCoursesProgres_Name",
                table: "AppCoursesProgres");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "AppQuizesApp");

            migrationBuilder.DropColumn(
                name: "IsQuiz",
                table: "AppCoursesProgres");
        }
    }
}
