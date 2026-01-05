using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dev.Acadmy.Migrations
{
    /// <inheritdoc />
    public partial class EaxmConfigurationMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppExamStudentsProgres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    TryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExamStudentsProgres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppExamStudentsProgres_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppExamStudentsProgres_AppExamsProgres_ExamId",
                        column: x => x.ExamId,
                        principalTable: "AppExamsProgres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppExamStudentAnswersProgres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamStudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedAnswerId = table.Column<Guid>(type: "uuid", nullable: true),
                    TextAnswer = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ScoreObtained = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExamStudentAnswersProgres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppExamStudentAnswersProgres_AppExamStudentsProgres_ExamStu~",
                        column: x => x.ExamStudentId,
                        principalTable: "AppExamStudentsProgres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppExamStudentAnswersProgres_AppQuestionesApp_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "AppQuestionesApp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppExamStudentAnswersProgres_ExamStudentId",
                table: "AppExamStudentAnswersProgres",
                column: "ExamStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExamStudentAnswersProgres_QuestionId",
                table: "AppExamStudentAnswersProgres",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExamStudentsProgres_ExamId",
                table: "AppExamStudentsProgres",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExamStudentsProgres_UserId_ExamId",
                table: "AppExamStudentsProgres",
                columns: new[] { "UserId", "ExamId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppExamStudentAnswersProgres");

            migrationBuilder.DropTable(
                name: "AppExamStudentsProgres");
        }
    }
}
