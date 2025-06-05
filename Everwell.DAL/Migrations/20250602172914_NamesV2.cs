using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class NamesV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointment_Service_service_id",
                schema: "EverWellDB_v1",
                table: "appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_appointment_Users_consultant_id",
                schema: "EverWellDB_v1",
                table: "appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_appointment_Users_customer_id",
                schema: "EverWellDB_v1",
                table: "appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_appointment_appointment_id",
                schema: "EverWellDB_v1",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_menstrual_cycle_notification_menstrual_cycle_tracking_track~",
                schema: "EverWellDB_v1",
                table: "menstrual_cycle_notification");

            migrationBuilder.DropForeignKey(
                name: "FK_menstrual_cycle_tracking_Users_customer_id",
                schema: "EverWellDB_v1",
                table: "menstrual_cycle_tracking");

            migrationBuilder.DropForeignKey(
                name: "FK_questions_Users_consultant_id",
                schema: "EverWellDB_v1",
                table: "questions");

            migrationBuilder.DropForeignKey(
                name: "FK_questions_Users_customer_id",
                schema: "EverWellDB_v1",
                table: "questions");

            migrationBuilder.DropForeignKey(
                name: "FK_STITesting_appointment_appointment_id",
                schema: "EverWellDB_v1",
                table: "STITesting");

            migrationBuilder.DropPrimaryKey(
                name: "PK_questions",
                schema: "EverWellDB_v1",
                table: "questions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_appointment",
                schema: "EverWellDB_v1",
                table: "appointment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_menstrual_cycle_tracking",
                schema: "EverWellDB_v1",
                table: "menstrual_cycle_tracking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_menstrual_cycle_notification",
                schema: "EverWellDB_v1",
                table: "menstrual_cycle_notification");

            migrationBuilder.RenameTable(
                name: "questions",
                schema: "EverWellDB_v1",
                newName: "Questions",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "appointment",
                schema: "EverWellDB_v1",
                newName: "Appointment",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "menstrual_cycle_tracking",
                schema: "EverWellDB_v1",
                newName: "MenstrualCycleTracking",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "menstrual_cycle_notification",
                schema: "EverWellDB_v1",
                newName: "MenstrualCycleNotification",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameIndex(
                name: "IX_questions_customer_id",
                schema: "EverWellDB_v1",
                table: "Questions",
                newName: "IX_Questions_customer_id");

            migrationBuilder.RenameIndex(
                name: "IX_questions_consultant_id",
                schema: "EverWellDB_v1",
                table: "Questions",
                newName: "IX_Questions_consultant_id");

            migrationBuilder.RenameIndex(
                name: "IX_appointment_service_id",
                schema: "EverWellDB_v1",
                table: "Appointment",
                newName: "IX_Appointment_service_id");

            migrationBuilder.RenameIndex(
                name: "IX_appointment_customer_id",
                schema: "EverWellDB_v1",
                table: "Appointment",
                newName: "IX_Appointment_customer_id");

            migrationBuilder.RenameIndex(
                name: "IX_appointment_consultant_id",
                schema: "EverWellDB_v1",
                table: "Appointment",
                newName: "IX_Appointment_consultant_id");

            migrationBuilder.RenameIndex(
                name: "IX_menstrual_cycle_tracking_customer_id",
                schema: "EverWellDB_v1",
                table: "MenstrualCycleTracking",
                newName: "IX_MenstrualCycleTracking_customer_id");

            migrationBuilder.RenameIndex(
                name: "IX_menstrual_cycle_notification_tracking_id",
                schema: "EverWellDB_v1",
                table: "MenstrualCycleNotification",
                newName: "IX_MenstrualCycleNotification_tracking_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Questions",
                schema: "EverWellDB_v1",
                table: "Questions",
                column: "question_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Appointment",
                schema: "EverWellDB_v1",
                table: "Appointment",
                column: "appointment_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MenstrualCycleTracking",
                schema: "EverWellDB_v1",
                table: "MenstrualCycleTracking",
                column: "tracking_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MenstrualCycleNotification",
                schema: "EverWellDB_v1",
                table: "MenstrualCycleNotification",
                column: "notification_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Service_service_id",
                schema: "EverWellDB_v1",
                table: "Appointment",
                column: "service_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Service",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Users_consultant_id",
                schema: "EverWellDB_v1",
                table: "Appointment",
                column: "consultant_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Users_customer_id",
                schema: "EverWellDB_v1",
                table: "Appointment",
                column: "customer_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Appointment_appointment_id",
                schema: "EverWellDB_v1",
                table: "Feedback",
                column: "appointment_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Appointment",
                principalColumn: "appointment_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenstrualCycleNotification_MenstrualCycleTracking_tracking_~",
                schema: "EverWellDB_v1",
                table: "MenstrualCycleNotification",
                column: "tracking_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "MenstrualCycleTracking",
                principalColumn: "tracking_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenstrualCycleTracking_Users_customer_id",
                schema: "EverWellDB_v1",
                table: "MenstrualCycleTracking",
                column: "customer_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_consultant_id",
                schema: "EverWellDB_v1",
                table: "Questions",
                column: "consultant_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_customer_id",
                schema: "EverWellDB_v1",
                table: "Questions",
                column: "customer_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_STITesting_Appointment_appointment_id",
                schema: "EverWellDB_v1",
                table: "STITesting",
                column: "appointment_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Appointment",
                principalColumn: "appointment_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Service_service_id",
                schema: "EverWellDB_v1",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Users_consultant_id",
                schema: "EverWellDB_v1",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Users_customer_id",
                schema: "EverWellDB_v1",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Appointment_appointment_id",
                schema: "EverWellDB_v1",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_MenstrualCycleNotification_MenstrualCycleTracking_tracking_~",
                schema: "EverWellDB_v1",
                table: "MenstrualCycleNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_MenstrualCycleTracking_Users_customer_id",
                schema: "EverWellDB_v1",
                table: "MenstrualCycleTracking");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_consultant_id",
                schema: "EverWellDB_v1",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_customer_id",
                schema: "EverWellDB_v1",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_STITesting_Appointment_appointment_id",
                schema: "EverWellDB_v1",
                table: "STITesting");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Questions",
                schema: "EverWellDB_v1",
                table: "Questions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Appointment",
                schema: "EverWellDB_v1",
                table: "Appointment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MenstrualCycleTracking",
                schema: "EverWellDB_v1",
                table: "MenstrualCycleTracking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MenstrualCycleNotification",
                schema: "EverWellDB_v1",
                table: "MenstrualCycleNotification");

            migrationBuilder.RenameTable(
                name: "Questions",
                schema: "EverWellDB_v1",
                newName: "questions",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "Appointment",
                schema: "EverWellDB_v1",
                newName: "appointment",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleTracking",
                schema: "EverWellDB_v1",
                newName: "menstrual_cycle_tracking",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleNotification",
                schema: "EverWellDB_v1",
                newName: "menstrual_cycle_notification",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_customer_id",
                schema: "EverWellDB_v1",
                table: "questions",
                newName: "IX_questions_customer_id");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_consultant_id",
                schema: "EverWellDB_v1",
                table: "questions",
                newName: "IX_questions_consultant_id");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_service_id",
                schema: "EverWellDB_v1",
                table: "appointment",
                newName: "IX_appointment_service_id");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_customer_id",
                schema: "EverWellDB_v1",
                table: "appointment",
                newName: "IX_appointment_customer_id");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_consultant_id",
                schema: "EverWellDB_v1",
                table: "appointment",
                newName: "IX_appointment_consultant_id");

            migrationBuilder.RenameIndex(
                name: "IX_MenstrualCycleTracking_customer_id",
                schema: "EverWellDB_v1",
                table: "menstrual_cycle_tracking",
                newName: "IX_menstrual_cycle_tracking_customer_id");

            migrationBuilder.RenameIndex(
                name: "IX_MenstrualCycleNotification_tracking_id",
                schema: "EverWellDB_v1",
                table: "menstrual_cycle_notification",
                newName: "IX_menstrual_cycle_notification_tracking_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_questions",
                schema: "EverWellDB_v1",
                table: "questions",
                column: "question_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_appointment",
                schema: "EverWellDB_v1",
                table: "appointment",
                column: "appointment_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_menstrual_cycle_tracking",
                schema: "EverWellDB_v1",
                table: "menstrual_cycle_tracking",
                column: "tracking_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_menstrual_cycle_notification",
                schema: "EverWellDB_v1",
                table: "menstrual_cycle_notification",
                column: "notification_id");

            migrationBuilder.AddForeignKey(
                name: "FK_appointment_Service_service_id",
                schema: "EverWellDB_v1",
                table: "appointment",
                column: "service_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Service",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_appointment_Users_consultant_id",
                schema: "EverWellDB_v1",
                table: "appointment",
                column: "consultant_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_appointment_Users_customer_id",
                schema: "EverWellDB_v1",
                table: "appointment",
                column: "customer_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_appointment_appointment_id",
                schema: "EverWellDB_v1",
                table: "Feedback",
                column: "appointment_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "appointment",
                principalColumn: "appointment_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_menstrual_cycle_notification_menstrual_cycle_tracking_track~",
                schema: "EverWellDB_v1",
                table: "menstrual_cycle_notification",
                column: "tracking_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "menstrual_cycle_tracking",
                principalColumn: "tracking_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_menstrual_cycle_tracking_Users_customer_id",
                schema: "EverWellDB_v1",
                table: "menstrual_cycle_tracking",
                column: "customer_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_questions_Users_consultant_id",
                schema: "EverWellDB_v1",
                table: "questions",
                column: "consultant_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_questions_Users_customer_id",
                schema: "EverWellDB_v1",
                table: "questions",
                column: "customer_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_STITesting_appointment_appointment_id",
                schema: "EverWellDB_v1",
                table: "STITesting",
                column: "appointment_id",
                principalSchema: "EverWellDB_v1",
                principalTable: "appointment",
                principalColumn: "appointment_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
