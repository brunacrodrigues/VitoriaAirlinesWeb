using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitoriaAirlinesWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddAirplaneStatusToAirplane : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Airplanes",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.Sql("UPDATE Airplanes SET Status = 'Active'");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Airplanes",
                type: "nvarchar(50)",
                nullable: false);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Airplanes");
        }
    }
}
