using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class updatePostEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_STITesting_Users_UserId",
            //     schema: "EverWellDB_v2.5",
            //     table: "STITesting");
            //
            // migrationBuilder.DropForeignKey(
            //     name: "FK_TestResults_Users_UserId",
            //     schema: "EverWellDB_v2.5",
            //     table: "TestResults");
            //
            // migrationBuilder.DropForeignKey(
            //     name: "FK_TestResults_Users_UserId1",
            //     schema: "EverWellDB_v2.5",
            //     table: "TestResults");
            //
            // migrationBuilder.DropIndex(
            //     name: "IX_TestResults_UserId",
            //     schema: "EverWellDB_v2.5",
            //     table: "TestResults");
            //
            // migrationBuilder.DropIndex(
            //     name: "IX_TestResults_UserId1",
            //     schema: "EverWellDB_v2.5",
            //     table: "TestResults");
            //
            // migrationBuilder.DropIndex(
            //     name: "IX_STITesting_UserId",
            //     schema: "EverWellDB_v2.5",
            //     table: "STITesting");
            //
            // migrationBuilder.DropColumn(
            //     name: "UserId",
            //     schema: "EverWellDB_v2.5",
            //     table: "TestResults");
            //
            // migrationBuilder.DropColumn(
            //     name: "UserId1",
            //     schema: "EverWellDB_v2.5",
            //     table: "TestResults");
            //
            // migrationBuilder.DropColumn(
            //     name: "UserId",
            //     schema: "EverWellDB_v2.5",
            //     table: "STITesting");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "EverWellDB_v2.5",
                table: "Post",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                schema: "EverWellDB_v2.5",
                table: "Post",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_url",
                schema: "EverWellDB_v2.5",
                table: "Post");

            // migrationBuilder.AddColumn<Guid>(
            //     name: "UserId",
            //     schema: "EverWellDB_v2.5",
            //     table: "TestResults",
            //     type: "uuid",
            //     nullable: true);
            //
            // migrationBuilder.AddColumn<Guid>(
            //     name: "UserId1",
            //     schema: "EverWellDB_v2.5",
            //     table: "TestResults",
            //     type: "uuid",
            //     nullable: true);
            //
            // migrationBuilder.AddColumn<Guid>(
            //     name: "UserId",
            //     schema: "EverWellDB_v2.5",
            //     table: "STITesting",
            //     type: "uuid",
            //     nullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "created_at",
                schema: "EverWellDB_v2.5",
                table: "Post",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            // migrationBuilder.CreateIndex(
            //     name: "IX_TestResults_UserId",
            //     schema: "EverWellDB_v2.5",
            //     table: "TestResults",
            //     column: "UserId");
            //
            // migrationBuilder.CreateIndex(
            //     name: "IX_TestResults_UserId1",
            //     schema: "EverWellDB_v2.5",
            //     table: "TestResults",
            //     column: "UserId1");
            //
            // migrationBuilder.CreateIndex(
            //     name: "IX_STITesting_UserId",
            //     schema: "EverWellDB_v2.5",
            //     table: "STITesting",
            //     column: "UserId");
            //
            // migrationBuilder.AddForeignKey(
            //     name: "FK_STITesting_Users_UserId",
            //     schema: "EverWellDB_v2.5",
            //     table: "STITesting",
            //     column: "UserId",
            //     principalSchema: "EverWellDB_v2.5",
            //     principalTable: "Users",
            //     principalColumn: "id");
            //
            // migrationBuilder.AddForeignKey(
            //     name: "FK_TestResults_Users_UserId",
            //     schema: "EverWellDB_v2.5",
            //     table: "TestResults",
            //     column: "UserId",
            //     principalSchema: "EverWellDB_v2.5",
            //     principalTable: "Users",
            //     principalColumn: "id");
            //
            // migrationBuilder.AddForeignKey(
            //     name: "FK_TestResults_Users_UserId1",
            //     schema: "EverWellDB_v2.5",
            //     table: "TestResults",
            //     column: "UserId1",
            //     principalSchema: "EverWellDB_v2.5",
            //     principalTable: "Users",
            //     principalColumn: "id");
        }
    }
}
