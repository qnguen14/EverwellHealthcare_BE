using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMenstrualCycleNotificationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "menstrual_cycle_tracking_id",
                schema: "EverWellDB_v2.5",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_menstrual_cycle_tracking_id",
                schema: "EverWellDB_v2.5",
                table: "Notifications",
                column: "menstrual_cycle_tracking_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_MenstrualCycleTracking_menstrual_cycle_tracki~",
                schema: "EverWellDB_v2.5",
                table: "Notifications",
                column: "menstrual_cycle_tracking_id",
                principalSchema: "EverWellDB_v2.5",
                principalTable: "MenstrualCycleTracking",
                principalColumn: "tracking_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_MenstrualCycleTracking_menstrual_cycle_tracki~",
                schema: "EverWellDB_v2.5",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_menstrual_cycle_tracking_id",
                schema: "EverWellDB_v2.5",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "menstrual_cycle_tracking_id",
                schema: "EverWellDB_v2.5",
                table: "Notifications");
        }
    }
}
