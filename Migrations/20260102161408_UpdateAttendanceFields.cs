using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttendanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttendanceStatus",
                table: "Employee",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendanceStatus",
                table: "Employee");
        }
    }
}
