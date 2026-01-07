using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dev.Acadmy.Migrations
{
    /// <inheritdoc />
    public partial class CertAndInstructorMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCertificateIssued",
                table: "AppExamStudentsProgres",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCertificateRequestDate",
                table: "AppExamStudentsProgres",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSenderInstructor",
                table: "AppChatMessagesApp",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCertificateIssued",
                table: "AppExamStudentsProgres");

            migrationBuilder.DropColumn(
                name: "LastCertificateRequestDate",
                table: "AppExamStudentsProgres");

            migrationBuilder.DropColumn(
                name: "IsSenderInstructor",
                table: "AppChatMessagesApp");
        }
    }
}
