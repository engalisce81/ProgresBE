using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dev.Acadmy.Migrations
{
    /// <inheritdoc />
    public partial class CourseCertificateMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TelegramVideoUrl",
                table: "AppLecturesApp",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppCourseCertificatesProgres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    NameXPosition = table.Column<double>(type: "double precision", nullable: false),
                    NameYPosition = table.Column<double>(type: "double precision", nullable: false),
                    FontSize = table.Column<float>(type: "real", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCourseCertificatesProgres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCourseCertificatesProgres_AppCoursesProgres_CourseId",
                        column: x => x.CourseId,
                        principalTable: "AppCoursesProgres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCourseCertificatesProgres_CourseId",
                table: "AppCourseCertificatesProgres",
                column: "CourseId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppCourseCertificatesProgres");

            migrationBuilder.DropColumn(
                name: "TelegramVideoUrl",
                table: "AppLecturesApp");
        }
    }
}
