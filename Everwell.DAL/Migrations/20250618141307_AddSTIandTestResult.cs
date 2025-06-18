using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSTIandTestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Skip altering non-existent tables
            // Instead, create the tables from scratch
            
            migrationBuilder.CreateTable(
                name: "STITesting",
                schema: "EverWellDB_v2",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_date = table.Column<DateOnly>(type: "date", nullable: false),
                    slot = table.Column<int>(type: "integer", nullable: false),
                    test_package = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STITesting", x => x.id);
                    table.ForeignKey(
                        name: "FK_STITesting_Users_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "EverWellDB_v2",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestResults",
                schema: "EverWellDB_v2",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sti_testing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parameter = table.Column<int>(type: "integer", nullable: false),
                    outcome = table.Column<string>(type: "text", nullable: false),
                    comments = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.id);
                    table.ForeignKey(
                        name: "FK_TestResults_STITesting_sti_testing_id",
                        column: x => x.sti_testing_id,
                        principalSchema: "EverWellDB_v2",
                        principalTable: "STITesting",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestResults_Users_staff_id",
                        column: x => x.staff_id,
                        principalSchema: "EverWellDB_v2",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_STITesting_customer_id",
                schema: "EverWellDB_v2",
                table: "STITesting",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_sti_testing_id",
                schema: "EverWellDB_v2",
                table: "TestResults",
                column: "sti_testing_id");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_staff_id",
                schema: "EverWellDB_v2",
                table: "TestResults",
                column: "staff_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestResults",
                schema: "EverWellDB_v2");

            migrationBuilder.DropTable(
                name: "STITesting",
                schema: "EverWellDB_v2");
        }
    }
}