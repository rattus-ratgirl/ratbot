using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RatBot.Infrastructure.Data;

#nullable disable

namespace RatBot.Infrastructure.Migrations;

/// <inheritdoc />
[DbContext(typeof(BotDbContext))]
[Migration("20260404000200_RemoveVirtueAndUserScores")]
public partial class RemoveVirtueAndUserScores : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""DROP TABLE IF EXISTS "UserScores";""");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserScores",
            columns: table => new
            {
                UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                Score = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserScores", x => x.UserId);
            }
        );
    }
}
