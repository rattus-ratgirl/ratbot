using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RatBot.Infrastructure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOperationalVirtueTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VirtueReactionLocks");

            migrationBuilder.DropTable(
                name: "VirtueRoleTierConfigs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VirtueReactionLocks",
                columns: table => new
                {
                    MessageId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ReactorUserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EmojiId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    TargetUserId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtueReactionLocks", x => new { x.MessageId, x.ReactorUserId });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VirtueRoleTierConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    TierIndex = table.Column<int>(type: "int", nullable: false),
                    MaxVirtue = table.Column<int>(type: "int", nullable: false),
                    MinVirtue = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtueRoleTierConfigs", x => new { x.GuildId, x.TierIndex });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_VirtueReactionLocks_TargetUserId",
                table: "VirtueReactionLocks",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtueRoleTierConfigs_GuildId",
                table: "VirtueRoleTierConfigs",
                column: "GuildId");
        }
    }
}
