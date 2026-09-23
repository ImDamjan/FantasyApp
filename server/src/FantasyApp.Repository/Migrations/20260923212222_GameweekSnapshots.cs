using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyApp.Repository.Migrations
{
    /// <inheritdoc />
    public partial class GameweekSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FantasyTeams_Gameweeks_LastFreeTransferGameweekId",
                table: "FantasyTeams");

            migrationBuilder.RenameColumn(
                name: "LastFreeTransferGameweekId",
                table: "FantasyTeams",
                newName: "LastSnapshotGameweekId");

            migrationBuilder.RenameIndex(
                name: "IX_FantasyTeams_LastFreeTransferGameweekId",
                table: "FantasyTeams",
                newName: "IX_FantasyTeams_LastSnapshotGameweekId");

            migrationBuilder.Sql("UPDATE Leagues SET Name = LEFT(Name, 20) WHERE LEN(Name) > 20;");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Leagues",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "ScoresFinalized",
                table: "Gameweeks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SquadsSnapshotted",
                table: "Gameweeks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GameweekPicks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FantasyTeamId = table.Column<long>(type: "bigint", nullable: false),
                    GameweekId = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    IsStarting = table.Column<bool>(type: "bit", nullable: false),
                    BenchOrder = table.Column<int>(type: "int", nullable: true),
                    IsCaptain = table.Column<bool>(type: "bit", nullable: false),
                    IsViceCaptain = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameweekPicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameweekPicks_FantasyTeams_FantasyTeamId",
                        column: x => x.FantasyTeamId,
                        principalTable: "FantasyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameweekPicks_Gameweeks_GameweekId",
                        column: x => x.GameweekId,
                        principalTable: "Gameweeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameweekPicks_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameweekPicks_FantasyTeamId_GameweekId_PlayerId",
                table: "GameweekPicks",
                columns: new[] { "FantasyTeamId", "GameweekId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameweekPicks_GameweekId",
                table: "GameweekPicks",
                column: "GameweekId");

            migrationBuilder.CreateIndex(
                name: "IX_GameweekPicks_PlayerId",
                table: "GameweekPicks",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyTeams_Gameweeks_LastSnapshotGameweekId",
                table: "FantasyTeams",
                column: "LastSnapshotGameweekId",
                principalTable: "Gameweeks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                DECLARE @PassedGameweeks TABLE (Id bigint PRIMARY KEY);
                INSERT INTO @PassedGameweeks SELECT Id FROM Gameweeks WHERE DeadlineTime <= SYSUTCDATETIME();

                UPDATE Gameweeks SET SquadsSnapshotted = 1 WHERE Id IN (SELECT Id FROM @PassedGameweeks);
                UPDATE Gameweeks SET ScoresFinalized = 1 WHERE IsFinished = 1;

                DELETE FROM UserGameweekScores WHERE GameweekId IN (SELECT Id FROM @PassedGameweeks);
                UPDATE UserGameweekScores SET TransferCost = 0, NetPoints = RawPoints;
                UPDATE Transfers SET WasFreeTransfer = 1 WHERE GameweekId NOT IN (SELECT Id FROM @PassedGameweeks);

                UPDATE FantasyTeams SET TripleCaptainUsed = 0 WHERE ActiveChip = 1 AND ActiveChipGameweekId IN (SELECT Id FROM @PassedGameweeks);
                UPDATE FantasyTeams SET BenchBoostUsed = 0 WHERE ActiveChip = 2 AND ActiveChipGameweekId IN (SELECT Id FROM @PassedGameweeks);
                UPDATE FantasyTeams SET WildCardUsed = 0 WHERE ActiveChip = 3 AND ActiveChipGameweekId IN (SELECT Id FROM @PassedGameweeks);
                UPDATE FantasyTeams SET ActiveChip = NULL, ActiveChipGameweekId = NULL WHERE ActiveChipGameweekId IN (SELECT Id FROM @PassedGameweeks);

                UPDATE FantasyTeams SET LastSnapshotGameweekId = NULL, FreeTransfersAvailable = 1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FantasyTeams_Gameweeks_LastSnapshotGameweekId",
                table: "FantasyTeams");

            migrationBuilder.DropTable(
                name: "GameweekPicks");

            migrationBuilder.DropColumn(
                name: "ScoresFinalized",
                table: "Gameweeks");

            migrationBuilder.DropColumn(
                name: "SquadsSnapshotted",
                table: "Gameweeks");

            migrationBuilder.RenameColumn(
                name: "LastSnapshotGameweekId",
                table: "FantasyTeams",
                newName: "LastFreeTransferGameweekId");

            migrationBuilder.RenameIndex(
                name: "IX_FantasyTeams_LastSnapshotGameweekId",
                table: "FantasyTeams",
                newName: "IX_FantasyTeams_LastFreeTransferGameweekId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Leagues",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyTeams_Gameweeks_LastFreeTransferGameweekId",
                table: "FantasyTeams",
                column: "LastFreeTransferGameweekId",
                principalTable: "Gameweeks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
