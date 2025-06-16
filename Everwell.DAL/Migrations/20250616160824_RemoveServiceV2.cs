using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveServiceV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "service_id",
                schema: "EverWellDB_v2",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "service_id",
                schema: "EverWellDB_v2",
                table: "Appointment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "service_id",
                schema: "EverWellDB_v2",
                table: "Feedback",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "service_id",
                schema: "EverWellDB_v2",
                table: "Appointment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
