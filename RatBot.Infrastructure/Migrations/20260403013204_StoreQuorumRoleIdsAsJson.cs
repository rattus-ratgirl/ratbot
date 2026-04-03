using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RatBot.Infrastructure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoreQuorumRoleIdsAsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "RoleIds", table: "QuorumScopeConfigs", type: "json", nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE `QuorumScopeConfigs`
                SET `RoleIds` = JSON_ARRAY(`RoleId`);
                """
            );

            migrationBuilder.AlterColumn<string>(
                name: "RoleIds",
                table: "QuorumScopeConfigs",
                type: "json",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true
            );

            migrationBuilder.DropColumn(name: "RoleId", table: "QuorumScopeConfigs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "RoleId",
                table: "QuorumScopeConfigs",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul
            );

            migrationBuilder.Sql(
                """
                UPDATE `QuorumScopeConfigs`
                SET `RoleId` = CAST(JSON_UNQUOTE(JSON_EXTRACT(`RoleIds`, '$[0]')) AS UNSIGNED)
                WHERE JSON_LENGTH(`RoleIds`) > 0;
                """
            );

            migrationBuilder.DropColumn(name: "RoleIds", table: "QuorumScopeConfigs");
        }
    }
}
