using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Incidents_AssignedToUserId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_TeamId",
                table: "Incidents");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_AssignedToUserId_CreatedAt",
                table: "Incidents",
                columns: new[] { "AssignedToUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_CreatedAt",
                table: "Incidents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ResolutionDueAt",
                table: "Incidents",
                column: "ResolutionDueAt");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_Status",
                table: "Incidents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_TeamId_CreatedAt",
                table: "Incidents",
                columns: new[] { "TeamId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Incidents_AssignedToUserId_CreatedAt",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_CreatedAt",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_ResolutionDueAt",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_Status",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_TeamId_CreatedAt",
                table: "Incidents");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_AssignedToUserId",
                table: "Incidents",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_TeamId",
                table: "Incidents",
                column: "TeamId");
        }
    }
}
