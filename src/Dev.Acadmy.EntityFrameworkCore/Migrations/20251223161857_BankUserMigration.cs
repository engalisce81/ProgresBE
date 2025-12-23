using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dev.Acadmy.Migrations
{
    /// <inheritdoc />
    public partial class BankUserMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppQuestionBanksApp_AppCoursesProgres_CourseId",
                table: "AppQuestionBanksApp");

            migrationBuilder.DropIndex(
                name: "IX_AppQuestionBanksApp_CourseId",
                table: "AppQuestionBanksApp");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "AppQuestionBanksApp");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AppQuestionBanksApp",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppQuestionBanksApp_UserId",
                table: "AppQuestionBanksApp",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppQuestionBanksApp_AbpUsers_UserId",
                table: "AppQuestionBanksApp",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppQuestionBanksApp_AbpUsers_UserId",
                table: "AppQuestionBanksApp");

            migrationBuilder.DropIndex(
                name: "IX_AppQuestionBanksApp_UserId",
                table: "AppQuestionBanksApp");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AppQuestionBanksApp");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "AppQuestionBanksApp",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppQuestionBanksApp_CourseId",
                table: "AppQuestionBanksApp",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppQuestionBanksApp_AppCoursesProgres_CourseId",
                table: "AppQuestionBanksApp",
                column: "CourseId",
                principalTable: "AppCoursesProgres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
