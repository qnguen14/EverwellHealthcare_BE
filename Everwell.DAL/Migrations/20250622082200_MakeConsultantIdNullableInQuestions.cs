using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MakeConsultantIdNullableInQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "consultant_id",
                schema: "EverWellDB_v2.5",
                table: "Questions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "question_id",
                schema: "EverWellDB_v2.5",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_question_id",
                schema: "EverWellDB_v2.5",
                table: "Notifications",
                column: "question_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Questions_question_id",
                schema: "EverWellDB_v2.5",
                table: "Notifications",
                column: "question_id",
                principalSchema: "EverWellDB_v2.5",
                principalTable: "Questions",
                principalColumn: "question_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Questions_question_id",
                schema: "EverWellDB_v2.5",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_question_id",
                schema: "EverWellDB_v2.5",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "question_id",
                schema: "EverWellDB_v2.5",
                table: "Notifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "consultant_id",
                schema: "EverWellDB_v2.5",
                table: "Questions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
