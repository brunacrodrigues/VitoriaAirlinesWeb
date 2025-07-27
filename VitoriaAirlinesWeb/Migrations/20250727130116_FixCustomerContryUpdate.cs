using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitoriaAirlinesWeb.Migrations
{
    /// <inheritdoc />
    public partial class FixCustomerContryUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerProfiles_Countries_CountryId",
                table: "CustomerProfiles");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerProfiles_Countries_CountryId",
                table: "CustomerProfiles",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerProfiles_Countries_CountryId",
                table: "CustomerProfiles");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerProfiles_Countries_CountryId",
                table: "CustomerProfiles",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");
        }
    }
}
