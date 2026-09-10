using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyApp.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddScoringAndChips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LastFreeTransferGameweekId",
                table: "FantasyTeams",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeams_LastFreeTransferGameweekId",
                table: "FantasyTeams",
                column: "LastFreeTransferGameweekId");

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyTeams_Gameweeks_LastFreeTransferGameweekId",
                table: "FantasyTeams",
                column: "LastFreeTransferGameweekId",
                principalTable: "Gameweeks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FantasyTeams_Gameweeks_LastFreeTransferGameweekId",
                table: "FantasyTeams");

            migrationBuilder.DropIndex(
                name: "IX_FantasyTeams_LastFreeTransferGameweekId",
                table: "FantasyTeams");

            migrationBuilder.DropColumn(
                name: "LastFreeTransferGameweekId",
                table: "FantasyTeams");
        }
    }
}
