using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitoriaAirlinesWeb.Migrations
{
    /// <inheritdoc />
    public partial class Replace_UniqueIndex_On_Tickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IX_Tickets_FlightId_SeatId ON Tickets");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IX_Tickets_FlightId_SeatId
                ON Tickets (FlightId, SeatId)
                WHERE IsCanceled = 0
            ");
        }



        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IX_Tickets_FlightId_SeatId ON Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_FlightId_SeatId",
                table: "Tickets",
                columns: new[] { "FlightId", "SeatId" },
                unique: true);
        }

    }
}
