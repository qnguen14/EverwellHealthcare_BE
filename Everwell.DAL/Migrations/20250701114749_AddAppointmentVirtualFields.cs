using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentVirtualFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "check_in_utc",
                schema: "EverWellDB_v2.5",
                table: "Appointment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "check_out_utc",
                schema: "EverWellDB_v2.5",
                table: "Appointment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""
        ALTER TABLE "EverWellDB_v2.5"."Appointment"
        ADD COLUMN IF NOT EXISTS google_event_id TEXT,
        ADD COLUMN IF NOT EXISTS google_meet_url TEXT,
        ADD COLUMN IF NOT EXISTS is_virtual     BOOLEAN DEFAULT FALSE,
        ADD COLUMN IF NOT EXISTS meeting_id     TEXT;
    """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "check_in_utc",
                schema: "EverWellDB_v2.5",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "check_out_utc",
                schema: "EverWellDB_v2.5",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "google_event_id",
                schema: "EverWellDB_v2.5",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "google_meet_url",
                schema: "EverWellDB_v2.5",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "is_virtual",
                schema: "EverWellDB_v2.5",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "meeting_id",
                schema: "EverWellDB_v2.5",
                table: "Appointment");
        }
    }
}
