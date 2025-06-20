using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitoriaAirlinesWeb.Migrations
{
    /// <inheritdoc />
    public partial class CorrectFlights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Airports_OriginId",
                table: "Flights");

            migrationBuilder.RenameColumn(
                name: "OriginId",
                table: "Flights",
                newName: "OriginAirportId");

            migrationBuilder.RenameIndex(
                name: "IX_Flights_OriginId",
                table: "Flights",
                newName: "IX_Flights_OriginAirportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Airports_OriginAirportId",
                table: "Flights",
                column: "OriginAirportId",
                principalTable: "Airports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Airports_OriginAirportId",
                table: "Flights");

            migrationBuilder.RenameColumn(
                name: "OriginAirportId",
                table: "Flights",
                newName: "OriginId");

            migrationBuilder.RenameIndex(
                name: "IX_Flights_OriginAirportId",
                table: "Flights",
                newName: "IX_Flights_OriginId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Airports_OriginId",
                table: "Flights",
                column: "OriginId",
                principalTable: "Airports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
