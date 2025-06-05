using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class fixedFeedbackv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_appointment_AppointmentId1",
                schema: "EverWellDB_v1",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_appointment_appointment_id",
                schema: "EverWellDB_v1",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Feedback_AppointmentId1",
                schema: "EverWellDB_v1",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "AppointmentId1",
                schema: "EverWellDB_v1",
                table: "Feedback");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_appointment_appointment_id",
                schema: "EverWellDB_v1",
                table: "Feedback",
                column: "appointment_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "appointment",
                principalColumn: "appointment_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_appointment_appointment_id",
                schema: "EverWellDB_v1",
                table: "Feedback");

            migrationBuilder.AddColumn<Guid>(
                name: "AppointmentId1",
                schema: "EverWellDB_v1",
                table: "Feedback",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_AppointmentId1",
                schema: "EverWellDB_v1",
                table: "Feedback",
                column: "AppointmentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_appointment_AppointmentId1",
                schema: "EverWellDB_v1",
                table: "Feedback",
                column: "AppointmentId1",
                principalSchema: "EverWellDB_v1",
                principalTable: "appointment",
                principalColumn: "appointment_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_appointment_appointment_id",
                schema: "EverWellDB_v1",
                table: "Feedback",
                column: "appointment_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "appointment",
                principalColumn: "appointment_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
