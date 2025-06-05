using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class newEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "EverWellDB_v1");

            migrationBuilder.CreateTable(
                name: "Service",
                schema: "EverWellDB_v1",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateOnly>(type: "date", nullable: false),
                    updated_at = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "EverWellDB_v1",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    avatar_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "appointment",
                schema: "EverWellDB_v1",
                columns: table => new
                {
                    appointment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consultant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    appointment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment", x => x.appointment_id);
                    table.ForeignKey(
                        name: "FK_appointment_Service_service_id",
                        column: x => x.service_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointment_Users_consultant_id",
                        column: x => x.consultant_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointment_Users_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "menstrual_cycle_tracking",
                schema: "EverWellDB_v1",
                columns: table => new
                {
                    tracking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cycle_start_date = table.Column<DateTime>(type: "date", nullable: false),
                    cycle_end_date = table.Column<DateTime>(type: "date", nullable: true),
                    symptoms = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notify_before_days = table.Column<int>(type: "integer", nullable: true),
                    notification_enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menstrual_cycle_tracking", x => x.tracking_id);
                    table.ForeignKey(
                        name: "FK_menstrual_cycle_tracking_Users_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Post",
                schema: "EverWellDB_v1",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    PostStatus = table.Column<int>(type: "integer", nullable: false),
                    PostCategory = table.Column<int>(type: "integer", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post", x => x.id);
                    table.ForeignKey(
                        name: "FK_Post_Users_staff_id",
                        column: x => x.staff_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                schema: "EverWellDB_v1",
                columns: table => new
                {
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consultant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    question_text = table.Column<string>(type: "text", nullable: false),
                    answer_text = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    answered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.question_id);
                    table.ForeignKey(
                        name: "FK_questions_Users_consultant_id",
                        column: x => x.consultant_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_questions_Users_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                schema: "EverWellDB_v1",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consultant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    appoinement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.id);
                    table.ForeignKey(
                        name: "FK_Feedback_Service_service_id",
                        column: x => x.service_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedback_Users_consultant_id",
                        column: x => x.consultant_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedback_Users_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedback_appointment_AppointmentId1",
                        column: x => x.AppointmentId1,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "appointment",
                        principalColumn: "appointment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedback_appointment_appoinement_id",
                        column: x => x.appoinement_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "appointment",
                        principalColumn: "appointment_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "STITesting",
                schema: "EverWellDB_v1",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    appointment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    test_type = table.Column<int>(type: "integer", nullable: false),
                    method = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    collected_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STITesting", x => x.id);
                    table.ForeignKey(
                        name: "FK_STITesting_Users_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STITesting_appointment_appointment_id",
                        column: x => x.appointment_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "appointment",
                        principalColumn: "appointment_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "menstrual_cycle_notification",
                schema: "EverWellDB_v1",
                columns: table => new
                {
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tracking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phase = table.Column<int>(type: "integer", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menstrual_cycle_notification", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_menstrual_cycle_notification_menstrual_cycle_tracking_track~",
                        column: x => x.tracking_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "menstrual_cycle_tracking",
                        principalColumn: "tracking_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestResults",
                schema: "EverWellDB_v1",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sti_testing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_data = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: true),
                    examined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.id);
                    table.ForeignKey(
                        name: "FK_TestResults_STITesting_sti_testing_id",
                        column: x => x.sti_testing_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "STITesting",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestResults_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_TestResults_Users_UserId1",
                        column: x => x.UserId1,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_TestResults_Users_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TestResults_Users_staff_id",
                        column: x => x.staff_id,
                        principalSchema: "EverWellDB_v1",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointment_consultant_id",
                schema: "EverWellDB_v1",
                table: "appointment",
                column: "consultant_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_customer_id",
                schema: "EverWellDB_v1",
                table: "appointment",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_service_id",
                schema: "EverWellDB_v1",
                table: "appointment",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_appoinement_id",
                schema: "EverWellDB_v1",
                table: "Feedback",
                column: "appoinement_id");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_AppointmentId1",
                schema: "EverWellDB_v1",
                table: "Feedback",
                column: "AppointmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_consultant_id",
                schema: "EverWellDB_v1",
                table: "Feedback",
                column: "consultant_id");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_customer_id",
                schema: "EverWellDB_v1",
                table: "Feedback",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_service_id",
                schema: "EverWellDB_v1",
                table: "Feedback",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_menstrual_cycle_notification_tracking_id",
                schema: "EverWellDB_v1",
                table: "menstrual_cycle_notification",
                column: "tracking_id");

            migrationBuilder.CreateIndex(
                name: "IX_menstrual_cycle_tracking_customer_id",
                schema: "EverWellDB_v1",
                table: "menstrual_cycle_tracking",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Post_staff_id",
                schema: "EverWellDB_v1",
                table: "Post",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_consultant_id",
                schema: "EverWellDB_v1",
                table: "questions",
                column: "consultant_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_customer_id",
                schema: "EverWellDB_v1",
                table: "questions",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_STITesting_appointment_id",
                schema: "EverWellDB_v1",
                table: "STITesting",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_STITesting_customer_id",
                schema: "EverWellDB_v1",
                table: "STITesting",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_customer_id",
                schema: "EverWellDB_v1",
                table: "TestResults",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_staff_id",
                schema: "EverWellDB_v1",
                table: "TestResults",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_sti_testing_id",
                schema: "EverWellDB_v1",
                table: "TestResults",
                column: "sti_testing_id");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_UserId",
                schema: "EverWellDB_v1",
                table: "TestResults",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_UserId1",
                schema: "EverWellDB_v1",
                table: "TestResults",
                column: "UserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feedback",
                schema: "EverWellDB_v1");

            migrationBuilder.DropTable(
                name: "menstrual_cycle_notification",
                schema: "EverWellDB_v1");

            migrationBuilder.DropTable(
                name: "Post",
                schema: "EverWellDB_v1");

            migrationBuilder.DropTable(
                name: "questions",
                schema: "EverWellDB_v1");

            migrationBuilder.DropTable(
                name: "TestResults",
                schema: "EverWellDB_v1");

            migrationBuilder.DropTable(
                name: "menstrual_cycle_tracking",
                schema: "EverWellDB_v1");

            migrationBuilder.DropTable(
                name: "STITesting",
                schema: "EverWellDB_v1");

            migrationBuilder.DropTable(
                name: "appointment",
                schema: "EverWellDB_v1");

            migrationBuilder.DropTable(
                name: "Service",
                schema: "EverWellDB_v1");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "EverWellDB_v1");
        }
    }
}
