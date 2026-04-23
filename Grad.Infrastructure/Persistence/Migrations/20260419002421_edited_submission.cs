using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class edited_submission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SubjectFK",
                table: "submissions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttemptsRemaining",
                table: "submissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "quizName",
                table: "submissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttemptsRemaining",
                table: "submissions");

            migrationBuilder.DropColumn(
                name: "quizName",
                table: "submissions");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubjectFK",
                table: "submissions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
