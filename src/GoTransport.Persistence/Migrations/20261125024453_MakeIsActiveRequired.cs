using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoTransport.Persistence.Migrations
{
    /// <summary>
    /// IS_ACTIVE becomes NOT NULL with no database default: entities start active in code (F-56).
    /// </summary>
    public partial class MakeIsActiveRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A NULL used to mean "active" (the column defaulted to 1 and saves filled it with true), so set
            // those rows to 1 before the column becomes NOT NULL; AlterColumn alone would make them inactive.
            migrationBuilder.Sql("UPDATE [APP_POINT] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [APP_RESERVATION] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [APP_ROLE] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [APP_ROLE_CLAIM] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [APP_ROUTE] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [APP_SCHEDULE] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [APP_USER] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [APP_USER_CLAIM] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [APP_USER_LOGIN] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [APP_USER_ROLE] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [APP_USER_TOKEN] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [APP_VEHICLE] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [BAS_CITY] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [BAS_DEPARTMENT] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [BAS_IDENTIFICATION_TYPE] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");
            migrationBuilder.Sql("UPDATE [BAS_MANUFACTURER] SET [IS_ACTIVE] = 1 WHERE [IS_ACTIVE] IS NULL;");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "BAS_MANUFACTURER",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "BAS_IDENTIFICATION_TYPE",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "BAS_DEPARTMENT",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "BAS_CITY",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_VEHICLE",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_USER_TOKEN",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_USER_ROLE",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_USER_LOGIN",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_USER_CLAIM",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_USER",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_SCHEDULE",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_ROUTE",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_ROLE_CLAIM",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_ROLE",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_RESERVATION",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_POINT",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "BAS_MANUFACTURER",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "BAS_IDENTIFICATION_TYPE",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "BAS_DEPARTMENT",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "BAS_CITY",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_VEHICLE",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_USER_TOKEN",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_USER_ROLE",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_USER_LOGIN",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_USER_CLAIM",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_USER",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_SCHEDULE",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_ROUTE",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_ROLE_CLAIM",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_ROLE",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_RESERVATION",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "APP_POINT",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
