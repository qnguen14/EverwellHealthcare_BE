using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleMeetToAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // google_meet_url already exists, so we only add the missing columns
            
            migrationBuilder.AddColumn<string>(
                name: "google_event_id",
                table: "Appointment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "meeting_id",
                table: "Appointment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_virtual",
                table: "Appointment",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only drop the columns we added, not google_meet_url which existed before
            
            migrationBuilder.DropColumn(
                name: "google_event_id",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "meeting_id",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "is_virtual",
                table: "Appointment");
        }
    }
} 