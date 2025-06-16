using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "EverWellDB_v1",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    notification_type = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    appointment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    test_result_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_Notifications_Appointment_appointment_id",
                        column: x => x.appointment_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Appointment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_TestResults_test_result_id",
                        column: x => x.test_result_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "TestResults",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_user_id",
                        column: x => x.user_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_appointment_id",
                schema: "EverWellDB_v1",
                table: "Notifications",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_created_at",
                schema: "EverWellDB_v1",
                table: "Notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_is_read",
                schema: "EverWellDB_v1",
                table: "Notifications",
                column: "is_read");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_test_result_id",
                schema: "EverWellDB_v1",
                table: "Notifications",
                column: "test_result_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_user_id",
                schema: "EverWellDB_v1",
                table: "Notifications",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "EverWellDB_v1");
        }
    }
}
