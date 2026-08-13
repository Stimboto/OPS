using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSlaManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncidentHistories_Users_ChangedByUserId",
                table: "IncidentHistories");

            migrationBuilder.AddColumn<DateTime>(
                name: "EscalatedAt",
                table: "Incidents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolutionDueAt",
                table: "Incidents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "ResolutionSlaBreached",
                table: "Incidents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolutionSlaWarningSentAt",
                table: "Incidents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResponseAt",
                table: "Incidents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResponseDueAt",
                table: "Incidents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "ResponseSlaBreached",
                table: "Incidents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResponseSlaWarningSentAt",
                table: "Incidents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ChangedByUserId",
                table: "IncidentHistories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentHistories_Users_ChangedByUserId",
                table: "IncidentHistories",
                column: "ChangedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncidentHistories_Users_ChangedByUserId",
                table: "IncidentHistories");

            migrationBuilder.DropColumn(
                name: "EscalatedAt",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ResolutionDueAt",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ResolutionSlaBreached",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ResolutionSlaWarningSentAt",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ResponseAt",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ResponseDueAt",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ResponseSlaBreached",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ResponseSlaWarningSentAt",
                table: "Incidents");

            migrationBuilder.AlterColumn<int>(
                name: "ChangedByUserId",
                table: "IncidentHistories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentHistories_Users_ChangedByUserId",
                table: "IncidentHistories",
                column: "ChangedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
