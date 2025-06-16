using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Everwell.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RolesMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "EverWellDB_v1",
                newName: "Users",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "TestResults",
                schema: "EverWellDB_v1",
                newName: "TestResults",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "STITesting",
                schema: "EverWellDB_v1",
                newName: "STITesting",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Service",
                schema: "EverWellDB_v1",
                newName: "Service",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Questions",
                schema: "EverWellDB_v1",
                newName: "Questions",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Post",
                schema: "EverWellDB_v1",
                newName: "Post",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Notifications",
                schema: "EverWellDB_v1",
                newName: "Notifications",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleTracking",
                schema: "EverWellDB_v1",
                newName: "MenstrualCycleTracking",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleNotification",
                schema: "EverWellDB_v1",
                newName: "MenstrualCycleNotification",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Feedback",
                schema: "EverWellDB_v1",
                newName: "Feedback",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "ConsultantSchedule",
                schema: "EverWellDB_v1",
                newName: "ConsultantSchedule",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "BlacklistedTokens",
                schema: "EverWellDB_v1",
                newName: "BlacklistedTokens",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameTable(
                name: "Appointment",
                schema: "EverWellDB_v1",
                newName: "Appointment",
                newSchema: "EverWellDB_v2");

            migrationBuilder.RenameColumn(
                name: "role",
                schema: "EverWellDB_v2",
                table: "Users",
                newName: "role_id");

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "EverWellDB_v2",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.id);
                });
            
            migrationBuilder.InsertData(
                schema: "EverWellDB_v2",
                table: "Roles",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, 0 }, // Customer
                    { 2, 1 }, // Consultant
                    { 3, 2 }, // Staff
                    { 4, 3 }, // Manager
                    { 5, 4 }  // Admin
                });
            
            migrationBuilder.Sql(@"
                UPDATE ""EverWellDB_v2"".""Users""
                SET role_id = 
                    CASE 
                        WHEN role_id = 0 THEN 1  -- Customer
                        WHEN role_id = 1 THEN 2  -- Consultant
                        WHEN role_id = 2 THEN 3  -- Staff
                        WHEN role_id = 3 THEN 4  -- Manager
                        WHEN role_id = 4 THEN 5  -- Admin
                        ELSE 1                   -- Default to Customer if invalid
                    END;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Users_role_id",
                schema: "EverWellDB_v2",
                table: "Users",
                column: "role_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_role_id",
                schema: "EverWellDB_v2",
                table: "Users",
                column: "role_id",
                principalSchema: "EverWellDB_v2",
                principalTable: "Roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
            
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_role_id",
                schema: "EverWellDB_v2",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "EverWellDB_v2");

            migrationBuilder.DropIndex(
                name: "IX_Users_role_id",
                schema: "EverWellDB_v2",
                table: "Users");

            migrationBuilder.EnsureSchema(
                name: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "EverWellDB_v2",
                newName: "Users",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "TestResults",
                schema: "EverWellDB_v2",
                newName: "TestResults",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "STITesting",
                schema: "EverWellDB_v2",
                newName: "STITesting",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "Service",
                schema: "EverWellDB_v2",
                newName: "Service",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "Questions",
                schema: "EverWellDB_v2",
                newName: "Questions",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "Post",
                schema: "EverWellDB_v2",
                newName: "Post",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "Notifications",
                schema: "EverWellDB_v2",
                newName: "Notifications",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleTracking",
                schema: "EverWellDB_v2",
                newName: "MenstrualCycleTracking",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "MenstrualCycleNotification",
                schema: "EverWellDB_v2",
                newName: "MenstrualCycleNotification",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "Feedback",
                schema: "EverWellDB_v2",
                newName: "Feedback",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "ConsultantSchedule",
                schema: "EverWellDB_v2",
                newName: "ConsultantSchedule",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "BlacklistedTokens",
                schema: "EverWellDB_v2",
                newName: "BlacklistedTokens",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameTable(
                name: "Appointment",
                schema: "EverWellDB_v2",
                newName: "Appointment",
                newSchema: "EverWellDB_v1");

            migrationBuilder.RenameColumn(
                name: "role_id",
                schema: "EverWellDB_v1",
                table: "Users",
                newName: "role");
        }
    }
}
