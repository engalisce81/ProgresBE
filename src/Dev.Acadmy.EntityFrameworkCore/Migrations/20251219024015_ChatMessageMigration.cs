using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dev.Acadmy.Migrations
{
    /// <inheritdoc />
    public partial class ChatMessageMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppChatMessagesApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppChatMessagesApp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppChatMessagesApp_AbpUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppChatMessagesApp_CreationTime",
                table: "AppChatMessagesApp",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatMessagesApp_ReceverId",
                table: "AppChatMessagesApp",
                column: "ReceverId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatMessagesApp_SenderId",
                table: "AppChatMessagesApp",
                column: "SenderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppChatMessagesApp");
        }
    }
}
