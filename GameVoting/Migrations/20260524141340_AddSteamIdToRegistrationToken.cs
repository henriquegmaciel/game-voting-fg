using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameVoting.Migrations
{
    /// <inheritdoc />
    public partial class AddSteamIdToRegistrationToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SteamId",
                table: "RegistrationTokens",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SteamId",
                table: "RegistrationTokens");
        }
    }
}
