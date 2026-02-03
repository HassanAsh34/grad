using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace grad.Migrations
{
    /// <inheritdoc />
    public partial class updatedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FName",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "LName",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "FName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "LName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FName",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "LName",
                table: "Parents");

            migrationBuilder.AddColumn<string>(
                name: "FName",
                table: "users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LName",
                table: "users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FName",
                table: "users");

            migrationBuilder.DropColumn(
                name: "LName",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "FName",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LName",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FName",
                table: "Parents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LName",
                table: "Parents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
