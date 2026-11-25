using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoTransport.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexesForDuplicateChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BAS_CITY_DEPARTMENT_ID",
                table: "BAS_CITY");

            migrationBuilder.DropIndex(
                name: "IX_APP_ROUTE_ORIGIN_POINT_ID",
                table: "APP_ROUTE");

            migrationBuilder.CreateIndex(
                name: "IX_BAS_DEPARTMENT_DESCRIPTION",
                table: "BAS_DEPARTMENT",
                column: "DESCRIPTION",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BAS_CITY_DEPARTMENT_ID_DESCRIPTION",
                table: "BAS_CITY",
                columns: new[] { "DEPARTMENT_ID", "DESCRIPTION" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_APP_VEHICLE_LICENSE_PLATE",
                table: "APP_VEHICLE",
                column: "LICENCE_PLATE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_APP_ROUTE_ORIGIN_POINT_ID_DESTINATION_POINT_ID",
                table: "APP_ROUTE",
                columns: new[] { "ORIGIN_POINT_ID", "DESTINATION_POINT_ID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BAS_DEPARTMENT_DESCRIPTION",
                table: "BAS_DEPARTMENT");

            migrationBuilder.DropIndex(
                name: "IX_BAS_CITY_DEPARTMENT_ID_DESCRIPTION",
                table: "BAS_CITY");

            migrationBuilder.DropIndex(
                name: "IX_APP_VEHICLE_LICENSE_PLATE",
                table: "APP_VEHICLE");

            migrationBuilder.DropIndex(
                name: "IX_APP_ROUTE_ORIGIN_POINT_ID_DESTINATION_POINT_ID",
                table: "APP_ROUTE");

            migrationBuilder.CreateIndex(
                name: "IX_BAS_CITY_DEPARTMENT_ID",
                table: "BAS_CITY",
                column: "DEPARTMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_APP_ROUTE_ORIGIN_POINT_ID",
                table: "APP_ROUTE",
                column: "ORIGIN_POINT_ID");
        }
    }
}
