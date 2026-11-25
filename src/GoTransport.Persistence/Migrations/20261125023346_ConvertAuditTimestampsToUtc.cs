using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoTransport.Persistence.Migrations
{
    /// <summary>
    /// Data-only: audit timestamps used to be stored in Colombian time (UTC−5, no daylight saving) and are
    /// now stored in UTC, so the rows written before this change move forward five hours.
    /// </summary>
    public partial class ConvertAuditTimestampsToUtc : Migration
    {
        private static readonly string[] AuditedTables =
        {
            "APP_POINT", "APP_RESERVATION", "APP_ROLE", "APP_ROLE_CLAIM", "APP_ROUTE", "APP_SCHEDULE",
            "APP_USER", "APP_USER_CLAIM", "APP_USER_LOGIN", "APP_USER_ROLE", "APP_USER_TOKEN", "APP_VEHICLE",
            "BAS_CITY", "BAS_DEPARTMENT", "BAS_IDENTIFICATION_TYPE", "BAS_MANUFACTURER"
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) => ShiftHours(migrationBuilder, 5);

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) => ShiftHours(migrationBuilder, -5);

        private static void ShiftHours(MigrationBuilder migrationBuilder, int hours)
        {
            foreach (var table in AuditedTables)
                migrationBuilder.Sql($"UPDATE [{table}] SET [CREATED_AT] = DATEADD(HOUR, {hours}, [CREATED_AT]), [UPDATED_AT] = DATEADD(HOUR, {hours}, [UPDATED_AT]);");
        }
    }
}
