using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tripcare360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Demo2EntityOverhaul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FlightNumber",
                table: "Claims",
                newName: "Tier");

            migrationBuilder.RenameColumn(
                name: "EstimatedPayout",
                table: "Claims",
                newName: "SubmittedAmount");

            migrationBuilder.AddColumn<string>(
                name: "AdminComments",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedPayout",
                table: "Claims",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ClaimCode",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileObjectKeys",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IncidentDetailsJson",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InsuredName",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPreValidationFailedDueToOutage",
                table: "Claims",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "Claims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Route",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminComments",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "CalculatedPayout",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ClaimCode",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "FileObjectKeys",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "IncidentDetailsJson",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "InsuredName",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "IsPreValidationFailedDueToOutage",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Route",
                table: "Claims");

            migrationBuilder.RenameColumn(
                name: "Tier",
                table: "Claims",
                newName: "FlightNumber");

            migrationBuilder.RenameColumn(
                name: "SubmittedAmount",
                table: "Claims",
                newName: "EstimatedPayout");
        }
    }
}
