using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class updatedappointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "end_time",
                schema: "EverWellDB_v1",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "start_time",
                schema: "EverWellDB_v1",
                table: "Appointment");

            migrationBuilder.AddColumn<int>(
                name: "shift_slot",
                schema: "EverWellDB_v1",
                table: "Appointment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ConsultantSchedule",
                schema: "EverWellDB_v1",
                columns: table => new
                {
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consultant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    work_date = table.Column<DateOnly>(type: "date", nullable: false),
                    shift_slot = table.Column<int>(type: "integer", nullable: false),
                    is_available = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultantSchedule", x => x.schedule_id);
                    table.ForeignKey(
                        name: "FK_ConsultantSchedule_Users_consultant_id",
                        column: x => x.consultant_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultantSchedule_consultant_id_work_date_shift_slot",
                schema: "EverWellDB_v1",
                table: "ConsultantSchedule",
                columns: new[] { "consultant_id", "work_date", "shift_slot" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultantSchedule",
                schema: "EverWellDB_v1");

            migrationBuilder.DropColumn(
                name: "shift_slot",
                schema: "EverWellDB_v1",
                table: "Appointment");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "end_time",
                schema: "EverWellDB_v1",
                table: "Appointment",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "start_time",
                schema: "EverWellDB_v1",
                table: "Appointment",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }
    }
}
