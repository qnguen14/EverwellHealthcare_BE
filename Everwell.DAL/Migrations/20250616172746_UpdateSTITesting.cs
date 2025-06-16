using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSTITesting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_STITesting_Users_customer_id",
                schema: "EverWellDB_v2",
                table: "STITesting");

            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_Users_customer_id",
                schema: "EverWellDB_v2",
                table: "TestResults");

            migrationBuilder.DropIndex(
                name: "IX_TestResults_customer_id",
                schema: "EverWellDB_v2",
                table: "TestResults");

            migrationBuilder.DropIndex(
                name: "IX_STITesting_customer_id",
                schema: "EverWellDB_v2",
                table: "STITesting");

            migrationBuilder.DropColumn(
                name: "customer_id",
                schema: "EverWellDB_v2",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "customer_id",
                schema: "EverWellDB_v2",
                table: "STITesting");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "EverWellDB_v2",
                table: "STITesting",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_STITesting_UserId",
                schema: "EverWellDB_v2",
                table: "STITesting",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_STITesting_Users_UserId",
                schema: "EverWellDB_v2",
                table: "STITesting",
                column: "UserId",
                principalSchema: "EverWellDB_v2",
                principalTable: "Users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_STITesting_Users_UserId",
                schema: "EverWellDB_v2",
                table: "STITesting");

            migrationBuilder.DropIndex(
                name: "IX_STITesting_UserId",
                schema: "EverWellDB_v2",
                table: "STITesting");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "EverWellDB_v2",
                table: "STITesting");

            migrationBuilder.AddColumn<Guid>(
                name: "customer_id",
                schema: "EverWellDB_v2",
                table: "TestResults",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "customer_id",
                schema: "EverWellDB_v2",
                table: "STITesting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_customer_id",
                schema: "EverWellDB_v2",
                table: "TestResults",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_STITesting_customer_id",
                schema: "EverWellDB_v2",
                table: "STITesting",
                column: "customer_id");

            migrationBuilder.AddForeignKey(
                name: "FK_STITesting_Users_customer_id",
                schema: "EverWellDB_v2",
                table: "STITesting",
                column: "customer_id",
                principalSchema: "EverWellDB_v2",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestResults_Users_customer_id",
                schema: "EverWellDB_v2",
                table: "TestResults",
                column: "customer_id",
                principalSchema: "EverWellDB_v2",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
