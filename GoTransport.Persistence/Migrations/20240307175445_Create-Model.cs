using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoTransport.Persistence.Migrations
{
    public partial class CreateModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APP_ROLE",
                columns: table => new
                {
                    ROLE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NAME = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    NORMALIZED_NAME = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    CONCURRENCY_STAMP = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_ROLE", x => x.ROLE_ID);
                });

            migrationBuilder.CreateTable(
                name: "APP_USER",
                columns: table => new
                {
                    USER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FIRST_NAME = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    LAST_NAME = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    USERNAME = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    NORMALIZED_USERNAME = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    EMAIL = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    NORMALIZED_EMAIL = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    EMAIL_CONFIRMED = table.Column<bool>(type: "bit", nullable: false),
                    PASSWORD_HASH = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    SECURITY_STAMP = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    CONCURRENCY_STAMP = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    PHONE_NUMBER = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    PHONE_NUMBER_CONFIRMED = table.Column<bool>(type: "bit", nullable: false),
                    TWO_FACTOR_ENABLED = table.Column<bool>(type: "bit", nullable: false),
                    LOCKOUT_END = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LOCKOUT_ENABLED = table.Column<bool>(type: "bit", nullable: false),
                    ACCESS_FAILED_COUNT = table.Column<int>(type: "int", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_USER", x => x.USER_ID);
                });

            migrationBuilder.CreateTable(
                name: "BAS_DEPARTMENT",
                columns: table => new
                {
                    DEPARTMENT_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DESCRIPTION = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAS_DEPARTMENT", x => x.DEPARTMENT_ID);
                });

            migrationBuilder.CreateTable(
                name: "BAS_IDENTIFICATION_TYPE",
                columns: table => new
                {
                    IDENTIFICATION_TYPE_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DESCRIPTION = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAS_IDENTIFICATION_TYPE", x => x.IDENTIFICATION_TYPE_ID);
                });

            migrationBuilder.CreateTable(
                name: "BAS_MANUFACTURER",
                columns: table => new
                {
                    MANUFACTURER_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DESCRIPTION = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAS_MANUFACTURER", x => x.MANUFACTURER_ID);
                });

            migrationBuilder.CreateTable(
                name: "APP_ROLE_CLAIM",
                columns: table => new
                {
                    CLAIM_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ROLE_ID = table.Column<long>(type: "bigint", nullable: false),
                    CLAIM_TYPE = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    CLAIM_VALUE = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_ROLE_CLAIM", x => x.CLAIM_ID);
                    table.ForeignKey(
                        name: "FK_APP_ROLE_CLAIM_ROLE_ID",
                        column: x => x.ROLE_ID,
                        principalTable: "APP_ROLE",
                        principalColumn: "ROLE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "APP_USER_CLAIM",
                columns: table => new
                {
                    CLAIM_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USER_ID = table.Column<long>(type: "bigint", nullable: false),
                    CLAIM_TYPE = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    CLAIM_VALUE = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_USER_CLAIM", x => x.CLAIM_ID);
                    table.ForeignKey(
                        name: "FK_APP_USER_CLAIM_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "APP_USER",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "APP_USER_LOGIN",
                columns: table => new
                {
                    LOGIN_PROVIDER = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    PROVIDER_KEY = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    PROVIDER_DISPLAY_NAME = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    USER_ID = table.Column<long>(type: "bigint", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_USER_LOGIN", x => new { x.LOGIN_PROVIDER, x.PROVIDER_KEY });
                    table.ForeignKey(
                        name: "FK_APP_USER_LOGIN_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "APP_USER",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "APP_USER_ROLE",
                columns: table => new
                {
                    USER_ID = table.Column<long>(type: "bigint", nullable: false),
                    ROLE_ID = table.Column<long>(type: "bigint", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_USER_ROLE", x => new { x.USER_ID, x.ROLE_ID });
                    table.ForeignKey(
                        name: "FK_APP_USER_ROLE_ROLE_ID",
                        column: x => x.ROLE_ID,
                        principalTable: "APP_ROLE",
                        principalColumn: "ROLE_ID");
                    table.ForeignKey(
                        name: "FK_APP_USER_ROLE_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "APP_USER",
                        principalColumn: "USER_ID");
                });

            migrationBuilder.CreateTable(
                name: "APP_USER_TOKEN",
                columns: table => new
                {
                    USER_ID = table.Column<long>(type: "bigint", nullable: false),
                    LOGIN_PROVIDER = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    NAME = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    VALUE = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_USER_TOKEN", x => new { x.USER_ID, x.LOGIN_PROVIDER, x.NAME });
                    table.ForeignKey(
                        name: "FK_APP_USER_TOKEN_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "APP_USER",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BAS_CITY",
                columns: table => new
                {
                    CITY_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DESCRIPTION = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    DEPARTMENT_ID = table.Column<int>(type: "int", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAS_CITY", x => x.CITY_ID);
                    table.ForeignKey(
                        name: "FK_BAS_CITY_BAS_DEPARTMENT_DEPARTMENT_ID",
                        column: x => x.DEPARTMENT_ID,
                        principalTable: "BAS_DEPARTMENT",
                        principalColumn: "DEPARTMENT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "APP_VEHICLE",
                columns: table => new
                {
                    VEHICLE_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MANUFACTURER_ID = table.Column<int>(type: "int", nullable: false),
                    LICENCE_PLATE = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    MODEL = table.Column<int>(type: "int", nullable: false),
                    CAPACITY = table.Column<int>(type: "int", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_VEHICLE", x => x.VEHICLE_ID);
                    table.ForeignKey(
                        name: "FK_APP_VEHICLE_MANUFACTURER_ID",
                        column: x => x.MANUFACTURER_ID,
                        principalTable: "BAS_MANUFACTURER",
                        principalColumn: "MANUFACTURER_ID");
                });

            migrationBuilder.CreateTable(
                name: "APP_POINT",
                columns: table => new
                {
                    POINT_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CITY_ID = table.Column<int>(type: "int", nullable: false),
                    DETAIL = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_POINT", x => x.POINT_ID);
                    table.ForeignKey(
                        name: "FK_APP_POINT_CITY_ID",
                        column: x => x.CITY_ID,
                        principalTable: "BAS_CITY",
                        principalColumn: "CITY_ID");
                });

            migrationBuilder.CreateTable(
                name: "APP_ROUTE",
                columns: table => new
                {
                    ROUTE_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DESCRIPTION = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    ORIGIN_POINT_ID = table.Column<int>(type: "int", nullable: false),
                    DESTINATION_POINT_ID = table.Column<int>(type: "int", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_ROUTE", x => x.ROUTE_ID);
                    table.ForeignKey(
                        name: "FK_APP_ROUTE_DESTINATION_POINT_ID",
                        column: x => x.DESTINATION_POINT_ID,
                        principalTable: "APP_POINT",
                        principalColumn: "POINT_ID");
                    table.ForeignKey(
                        name: "FK_APP_ROUTE_ORIGIN_POINT_ID",
                        column: x => x.ORIGIN_POINT_ID,
                        principalTable: "APP_POINT",
                        principalColumn: "POINT_ID");
                });

            migrationBuilder.CreateTable(
                name: "APP_SCHEDULE",
                columns: table => new
                {
                    SCHEDULE_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DEPARTURE_TIME = table.Column<TimeSpan>(type: "time(5)", maxLength: 5, nullable: false),
                    ARRIVAL_TIME = table.Column<TimeSpan>(type: "time(5)", maxLength: 5, nullable: false),
                    DURATION = table.Column<TimeSpan>(type: "time(5)", maxLength: 5, nullable: false),
                    ROUTE_ID = table.Column<int>(type: "int", nullable: false),
                    VEHICLE_ID = table.Column<int>(type: "int", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_SCHEDULE", x => x.SCHEDULE_ID);
                    table.ForeignKey(
                        name: "FK_APP_SCHEDULE_ROUTE_ID",
                        column: x => x.ROUTE_ID,
                        principalTable: "APP_ROUTE",
                        principalColumn: "ROUTE_ID");
                    table.ForeignKey(
                        name: "FK_APP_SCHEDULE_VEHICLE_ID",
                        column: x => x.VEHICLE_ID,
                        principalTable: "APP_VEHICLE",
                        principalColumn: "VEHICLE_ID");
                });

            migrationBuilder.CreateTable(
                name: "APP_RESERVATION",
                columns: table => new
                {
                    RESERVATION_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SCHEDULE_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RESERVATION_DATE = table.Column<DateTime>(type: "date", nullable: false),
                    PASSENGER_FIRST_NAME = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    PASSENGER_LAST_NAME = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    IDENTIFICATION_TYPE_ID = table.Column<int>(type: "int", nullable: false),
                    PASSENGER_IDENTIFICATION = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    DETAIL = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CREATION_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    UPDATE_USER = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_RESERVATION", x => x.RESERVATION_ID);
                    table.ForeignKey(
                        name: "FK_APP_RESERVATION_IDENTIFICATION_TYPE_ID",
                        column: x => x.IDENTIFICATION_TYPE_ID,
                        principalTable: "BAS_IDENTIFICATION_TYPE",
                        principalColumn: "IDENTIFICATION_TYPE_ID");
                    table.ForeignKey(
                        name: "FK_APP_RESERVATION_SCHEDULE_ID",
                        column: x => x.SCHEDULE_ID,
                        principalTable: "APP_SCHEDULE",
                        principalColumn: "SCHEDULE_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_APP_POINT_CITY_ID",
                table: "APP_POINT",
                column: "CITY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_APP_RESERVATION_IDENTIFICATION_TYPE_ID",
                table: "APP_RESERVATION",
                column: "IDENTIFICATION_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_APP_RESERVATION_SCHEDULE_ID",
                table: "APP_RESERVATION",
                column: "SCHEDULE_ID");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "APP_ROLE",
                column: "NORMALIZED_NAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_APP_ROLE_CLAIM_ROLE_ID",
                table: "APP_ROLE_CLAIM",
                column: "ROLE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_APP_ROUTE_DESTINATION_POINT_ID",
                table: "APP_ROUTE",
                column: "DESTINATION_POINT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_APP_ROUTE_ORIGIN_POINT_ID",
                table: "APP_ROUTE",
                column: "ORIGIN_POINT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_APP_SCHEDULE_ROUTE_ID",
                table: "APP_SCHEDULE",
                column: "ROUTE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_APP_SCHEDULE_VEHICLE_ID",
                table: "APP_SCHEDULE",
                column: "VEHICLE_ID");

            migrationBuilder.CreateIndex(
                name: "IDX_APP_USER_NORMALIZED_EMAIL",
                table: "APP_USER",
                column: "NORMALIZED_EMAIL");

            migrationBuilder.CreateIndex(
                name: "IDX_APP_USER_NORMALIZED_USERNAME",
                table: "APP_USER",
                column: "NORMALIZED_USERNAME",
                unique: true,
                filter: "[NORMALIZED_USERNAME] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IDX_APP_USER_CLAIM_USER_ID",
                table: "APP_USER_CLAIM",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IDX_APP_USER_LOGIN_USER_ID",
                table: "APP_USER_LOGIN",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IDX_APP_USER_ROLE_ROLE_ID",
                table: "APP_USER_ROLE",
                column: "ROLE_ID");

            migrationBuilder.CreateIndex(
                name: "IDX_APP_USER_ROLE_USER_ID",
                table: "APP_USER_ROLE",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IDX_APP_USER_TOKEN_USER_ID",
                table: "APP_USER_TOKEN",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_APP_VEHICLE_MANUFACTURER_ID",
                table: "APP_VEHICLE",
                column: "MANUFACTURER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_BAS_CITY_DEPARTMENT_ID",
                table: "BAS_CITY",
                column: "DEPARTMENT_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APP_RESERVATION");

            migrationBuilder.DropTable(
                name: "APP_ROLE_CLAIM");

            migrationBuilder.DropTable(
                name: "APP_USER_CLAIM");

            migrationBuilder.DropTable(
                name: "APP_USER_LOGIN");

            migrationBuilder.DropTable(
                name: "APP_USER_ROLE");

            migrationBuilder.DropTable(
                name: "APP_USER_TOKEN");

            migrationBuilder.DropTable(
                name: "BAS_IDENTIFICATION_TYPE");

            migrationBuilder.DropTable(
                name: "APP_SCHEDULE");

            migrationBuilder.DropTable(
                name: "APP_ROLE");

            migrationBuilder.DropTable(
                name: "APP_USER");

            migrationBuilder.DropTable(
                name: "APP_ROUTE");

            migrationBuilder.DropTable(
                name: "APP_VEHICLE");

            migrationBuilder.DropTable(
                name: "APP_POINT");

            migrationBuilder.DropTable(
                name: "BAS_MANUFACTURER");

            migrationBuilder.DropTable(
                name: "BAS_CITY");

            migrationBuilder.DropTable(
                name: "BAS_DEPARTMENT");
        }
    }
}