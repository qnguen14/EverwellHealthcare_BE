using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class updateNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "stitesting_id",
                schema: "EverWellDB_v2",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_stitesting_id",
                schema: "EverWellDB_v2",
                table: "Notifications",
                column: "stitesting_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_STITesting_stitesting_id",
                schema: "EverWellDB_v2",
                table: "Notifications",
                column: "stitesting_id",
                principalSchema: "EverWellDB_v2",
                principalTable: "STITesting",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_STITesting_stitesting_id",
                schema: "EverWellDB_v2",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_stitesting_id",
                schema: "EverWellDB_v2",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "stitesting_id",
                schema: "EverWellDB_v2",
                table: "Notifications");
        }
    }
}
