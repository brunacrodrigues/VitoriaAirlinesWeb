using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitoriaAirlinesWeb.Migrations
{
    /// <inheritdoc />
    public partial class CustomerCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "CustomerProfiles");

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "CustomerProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfiles_CountryId",
                table: "CustomerProfiles",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerProfiles_Countries_CountryId",
                table: "CustomerProfiles",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerProfiles_Countries_CountryId",
                table: "CustomerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProfiles_CountryId",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "CustomerProfiles");

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "CustomerProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
