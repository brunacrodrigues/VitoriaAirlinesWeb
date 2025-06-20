using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitoriaAirlinesWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightStatusToFlight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Flights",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.Sql("UPDATE Flights SET Status = 'Completed' WHERE DepartureUtc < GETDATE()");
            migrationBuilder.Sql("UPDATE Flights SET Status = 'Scheduled' WHERE DepartureUtc >= GETDATE() OR DepartureUtc IS NULL");

            migrationBuilder.AlterColumn<string>(
        name: "Status",
        table: "Flights",
        type: "nvarchar(50)",
        nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Flights");
        }
    }
}
