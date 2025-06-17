using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class updateSTI_TestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_STITesting_Appointment_appointment_id",
                schema: "EverWellDB_v2",
                table: "STITesting");

            migrationBuilder.DropColumn(
                name: "examined_at",
                schema: "EverWellDB_v2",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "result_data",
                schema: "EverWellDB_v2",
                table: "TestResults");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "EverWellDB_v2",
                table: "TestResults",
                newName: "outcome");

            migrationBuilder.RenameColumn(
                name: "sent_at",
                schema: "EverWellDB_v2",
                table: "TestResults",
                newName: "processed_at");

            migrationBuilder.RenameColumn(
                name: "test_type",
                schema: "EverWellDB_v2",
                table: "STITesting",
                newName: "test_package");

            migrationBuilder.RenameColumn(
                name: "method",
                schema: "EverWellDB_v2",
                table: "STITesting",
                newName: "slot");

            migrationBuilder.RenameColumn(
                name: "appointment_id",
                schema: "EverWellDB_v2",
                table: "STITesting",
                newName: "customer_id");

            migrationBuilder.RenameIndex(
                name: "IX_STITesting_appointment_id",
                schema: "EverWellDB_v2",
                table: "STITesting",
                newName: "IX_STITesting_customer_id");

            migrationBuilder.AddColumn<string>(
                name: "comments",
                schema: "EverWellDB_v2",
                table: "TestResults",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int[]>(
                name: "parameter",
                schema: "EverWellDB_v2",
                table: "TestResults",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "collected_date",
                schema: "EverWellDB_v2",
                table: "STITesting",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                schema: "EverWellDB_v2",
                table: "STITesting",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "EverWellDB_v2",
                table: "STITesting",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "is_completed",
                schema: "EverWellDB_v2",
                table: "STITesting",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                schema: "EverWellDB_v2",
                table: "STITesting",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "sample_taken_at",
                schema: "EverWellDB_v2",
                table: "STITesting",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_STITesting_Users_customer_id",
                schema: "EverWellDB_v2",
                table: "STITesting",
                column: "customer_id",
                principalSchema: "EverWellDB_v2",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_STITesting_Users_customer_id",
                schema: "EverWellDB_v2",
                table: "STITesting");

            migrationBuilder.DropColumn(
                name: "comments",
                schema: "EverWellDB_v2",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "parameter",
                schema: "EverWellDB_v2",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "completed_at",
                schema: "EverWellDB_v2",
                table: "STITesting");

            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "EverWellDB_v2",
                table: "STITesting");

            migrationBuilder.DropColumn(
                name: "is_completed",
                schema: "EverWellDB_v2",
                table: "STITesting");

            migrationBuilder.DropColumn(
                name: "notes",
                schema: "EverWellDB_v2",
                table: "STITesting");

            migrationBuilder.DropColumn(
                name: "sample_taken_at",
                schema: "EverWellDB_v2",
                table: "STITesting");

            migrationBuilder.RenameColumn(
                name: "processed_at",
                schema: "EverWellDB_v2",
                table: "TestResults",
                newName: "sent_at");

            migrationBuilder.RenameColumn(
                name: "outcome",
                schema: "EverWellDB_v2",
                table: "TestResults",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "test_package",
                schema: "EverWellDB_v2",
                table: "STITesting",
                newName: "test_type");

            migrationBuilder.RenameColumn(
                name: "slot",
                schema: "EverWellDB_v2",
                table: "STITesting",
                newName: "method");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                schema: "EverWellDB_v2",
                table: "STITesting",
                newName: "appointment_id");

            migrationBuilder.RenameIndex(
                name: "IX_STITesting_customer_id",
                schema: "EverWellDB_v2",
                table: "STITesting",
                newName: "IX_STITesting_appointment_id");

            migrationBuilder.AddColumn<DateTime>(
                name: "examined_at",
                schema: "EverWellDB_v2",
                table: "TestResults",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "result_data",
                schema: "EverWellDB_v2",
                table: "TestResults",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "collected_date",
                schema: "EverWellDB_v2",
                table: "STITesting",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddForeignKey(
                name: "FK_STITesting_Appointment_appointment_id",
                schema: "EverWellDB_v2",
                table: "STITesting",
                column: "appointment_id",
                principalSchema: "EverWellDB_v2",
                principalTable: "Appointment",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
