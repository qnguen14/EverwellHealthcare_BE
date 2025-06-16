using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Service_service_id",
                schema: "EverWellDB_v2",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Service_service_id",
                schema: "EverWellDB_v2",
                table: "Feedback");

            migrationBuilder.DropTable(
                name: "Service",
                schema: "EverWellDB_v2");

            migrationBuilder.DropIndex(
                name: "IX_Feedback_service_id",
                schema: "EverWellDB_v2",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_service_id",
                schema: "EverWellDB_v2",
                table: "Appointment");

        //     migrationBuilder.AddColumn<bool>(
        //         name: "is_sent",
        //         schema: "EverWellDB_v2",
        //         table: "MenstrualCycleNotification",
        //         type: "boolean",
        //         nullable: false,
        //         defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "is_sent",
            //     schema: "EverWellDB_v2",
            //     table: "MenstrualCycleNotification");

            migrationBuilder.CreateTable(
                name: "Service",
                schema: "EverWellDB_v2",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateOnly>(type: "date", nullable: false),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    updated_at = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_service_id",
                schema: "EverWellDB_v2",
                table: "Feedback",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_service_id",
                schema: "EverWellDB_v2",
                table: "Appointment",
                column: "service_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Service_service_id",
                schema: "EverWellDB_v2",
                table: "Appointment",
                column: "service_id",
                principalSchema: "EverWellDB_v2",
                principalTable: "Service",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Service_service_id",
                schema: "EverWellDB_v2",
                table: "Feedback",
                column: "service_id",
                principalSchema: "EverWellDB_v2",
                principalTable: "Service",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
