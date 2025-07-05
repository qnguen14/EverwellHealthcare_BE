using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class changeSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "TestResults",
                newName: "TestResults",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "STITesting",
                newName: "STITesting",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Roles",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Questions",
                newName: "Questions",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Post",
                newName: "Post",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "PaymentTransactions",
                newName: "PaymentTransactions",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "Notifications",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleTracking",
                newName: "MenstrualCycleTracking",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleNotification",
                newName: "MenstrualCycleNotification",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Feedback",
                newName: "Feedback",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "ConsultantSchedule",
                newName: "ConsultantSchedule",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "BlacklistedTokens",
                newName: "BlacklistedTokens",
                newSchema: "EverWellDB_v2.5");

            migrationBuilder.RenameTable(
                name: "Appointment",
                newName: "Appointment",
                newSchema: "EverWellDB_v2.5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Users",
                schema: "EverWellDB_v2.5",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "TestResults",
                schema: "EverWellDB_v2.5",
                newName: "TestResults");

            migrationBuilder.RenameTable(
                name: "STITesting",
                schema: "EverWellDB_v2.5",
                newName: "STITesting");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "EverWellDB_v2.5",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "Questions",
                schema: "EverWellDB_v2.5",
                newName: "Questions");

            migrationBuilder.RenameTable(
                name: "Post",
                schema: "EverWellDB_v2.5",
                newName: "Post");

            migrationBuilder.RenameTable(
                name: "PaymentTransactions",
                schema: "EverWellDB_v2.5",
                newName: "PaymentTransactions");

            migrationBuilder.RenameTable(
                name: "Notifications",
                schema: "EverWellDB_v2.5",
                newName: "Notifications");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleTracking",
                schema: "EverWellDB_v2.5",
                newName: "MenstrualCycleTracking");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleNotification",
                schema: "EverWellDB_v2.5",
                newName: "MenstrualCycleNotification");

            migrationBuilder.RenameTable(
                name: "Feedback",
                schema: "EverWellDB_v2.5",
                newName: "Feedback");

            migrationBuilder.RenameTable(
                name: "ConsultantSchedule",
                schema: "EverWellDB_v2.5",
                newName: "ConsultantSchedule");

            migrationBuilder.RenameTable(
                name: "BlacklistedTokens",
                schema: "EverWellDB_v2.5",
                newName: "BlacklistedTokens");

            migrationBuilder.RenameTable(
                name: "Appointment",
                schema: "EverWellDB_v2.5",
                newName: "Appointment");
        }
    }
}
