using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "EverWellDB_v2",
                newName: "Users",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "TestResults",
                schema: "EverWellDB_v2",
                newName: "TestResults",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "STITesting",
                schema: "EverWellDB_v2",
                newName: "STITesting",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "EverWellDB_v2",
                newName: "Roles",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Questions",
                schema: "EverWellDB_v2",
                newName: "Questions",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Post",
                schema: "EverWellDB_v2",
                newName: "Post",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Notifications",
                schema: "EverWellDB_v2",
                newName: "Notifications",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleTracking",
                schema: "EverWellDB_v2",
                newName: "MenstrualCycleTracking",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleNotification",
                schema: "EverWellDB_v2",
                newName: "MenstrualCycleNotification",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Feedback",
                schema: "EverWellDB_v2",
                newName: "Feedback",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "ConsultantSchedule",
                schema: "EverWellDB_v2",
                newName: "ConsultantSchedule",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "BlacklistedTokens",
                schema: "EverWellDB_v2",
                newName: "BlacklistedTokens",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Appointment",
                schema: "EverWellDB_v2",
                newName: "Appointment",
                newSchema: "EverWellDB_v2.5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "EverWellDB_v2.5",
                newName: "Users",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "TestResults",
                schema: "EverWellDB_v2.5",
                newName: "TestResults",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "STITesting",
                schema: "EverWellDB_v2.5",
                newName: "STITesting",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "EverWellDB_v2.5",
                newName: "Roles",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Questions",
                schema: "EverWellDB_v2.5",
                newName: "Questions",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Post",
                schema: "EverWellDB_v2.5",
                newName: "Post",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Notifications",
                schema: "EverWellDB_v2.5",
                newName: "Notifications",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleTracking",
                schema: "EverWellDB_v2.5",
                newName: "MenstrualCycleTracking",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleNotification",
                schema: "EverWellDB_v2.5",
                newName: "MenstrualCycleNotification",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Feedback",
                schema: "EverWellDB_v2.5",
                newName: "Feedback",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "ConsultantSchedule",
                schema: "EverWellDB_v2.5",
                newName: "ConsultantSchedule",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "BlacklistedTokens",
                schema: "EverWellDB_v2.5",
                newName: "BlacklistedTokens",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Appointment",
                schema: "EverWellDB_v2.5",
                newName: "Appointment",
                newSchema: "EverWellDB_v2");
        }
    }
}
