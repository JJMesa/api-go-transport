using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoTransport.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTripIndexToReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_APP_RESERVATION_SCHEDULE_ID",
                table: "APP_RESERVATION");

            migrationBuilder.CreateIndex(
                name: "IX_APP_RESERVATION_SCHEDULE_ID_RESERVATION_DATE",
                table: "APP_RESERVATION",
                columns: new[] { "SCHEDULE_ID", "RESERVATION_DATE" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_APP_RESERVATION_SCHEDULE_ID_RESERVATION_DATE",
                table: "APP_RESERVATION");

            migrationBuilder.CreateIndex(
                name: "IX_APP_RESERVATION_SCHEDULE_ID",
                table: "APP_RESERVATION",
                column: "SCHEDULE_ID");
        }
    }
}
