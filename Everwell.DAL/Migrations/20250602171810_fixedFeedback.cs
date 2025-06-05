using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class fixedFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_appointment_appoinement_id",
                schema: "EverWellDB_v1",
                table: "Feedback");

            migrationBuilder.RenameColumn(
                name: "appoinement_id",
                schema: "EverWellDB_v1",
                table: "Feedback",
                newName: "appointment_id");

            migrationBuilder.RenameIndex(
                name: "IX_Feedback_appoinement_id",
                schema: "EverWellDB_v1",
                table: "Feedback",
                newName: "IX_Feedback_appointment_id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_appointment_appointment_id",
                schema: "EverWellDB_v1",
                table: "Feedback");

            migrationBuilder.RenameColumn(
                name: "appointment_id",
                schema: "EverWellDB_v1",
                table: "Feedback",
                newName: "appoinement_id");

            migrationBuilder.RenameIndex(
                name: "IX_Feedback_appointment_id",
                schema: "EverWellDB_v1",
                table: "Feedback",
                newName: "IX_Feedback_appoinement_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_appointment_appoinement_id",
                schema: "EverWellDB_v1",
                table: "Feedback",
                column: "appoinement_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "appointment",
                principalColumn: "appointment_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
