using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameVoting.Migrations
{
    /// <inheritdoc />
    public partial class AddSteamIdAndAdminAlias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminAlias",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SteamId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SteamId",
                table: "AspNetUsers",
                column: "SteamId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SteamId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AdminAlias",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SteamId",
                table: "AspNetUsers");
        }
    }
}
