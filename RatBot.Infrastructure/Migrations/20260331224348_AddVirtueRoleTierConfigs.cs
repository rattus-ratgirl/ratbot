using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RatBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVirtueRoleTierConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .CreateTable(
                    name: "VirtueRoleTierConfigs",
                    columns: table => new
                    {
                        GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                        TierIndex = table.Column<int>(type: "int", nullable: false),
                        RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                        MinVirtue = table.Column<int>(type: "int", nullable: false),
                        MaxVirtue = table.Column<int>(type: "int", nullable: false),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_VirtueRoleTierConfigs", x => new { x.GuildId, x.TierIndex });
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(name: "IX_VirtueRoleTierConfigs_GuildId", table: "VirtueRoleTierConfigs", column: "GuildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "VirtueRoleTierConfigs");
        }
    }
}
