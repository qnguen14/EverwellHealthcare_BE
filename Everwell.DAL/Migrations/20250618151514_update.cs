using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "collected_date",
                schema: "EverWellDB_v2",
                table: "STITesting",
                newName: "schedule_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "schedule_date",
                schema: "EverWellDB_v2",
                table: "STITesting",
                newName: "collected_date");
        }
    }
}
