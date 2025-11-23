using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace grad.Migrations
{
    /// <inheritdoc />
    public partial class v5_subjectEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Subject_SubjectId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Subject_SubjectFK",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_SubjectId",
                table: "Students");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Subject_Name",
                table: "Subject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subject",
                table: "Subject");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "Students");

            migrationBuilder.RenameTable(
                name: "Subject",
                newName: "subjects");

            migrationBuilder.AlterColumn<string>(
                name: "SubjectName",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "subjects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<DateOnly>(
                name: "CreatedAt",
                table: "subjects",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "UpdatedAt",
                table: "subjects",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<bool>(
                name: "deaf_mute",
                table: "subjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_subjects",
                table: "subjects",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EnrolledSubjects",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    STUFK = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SUBFK = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrolledSubjects", x => x.Id);
                    table.UniqueConstraint("AK_EnrolledSubjects_STUFK_SUBFK", x => new { x.STUFK, x.SUBFK });
                    table.ForeignKey(
                        name: "FK_EnrolledSubjects_Students_STUFK",
                        column: x => x.STUFK,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnrolledSubjects_subjects_SUBFK",
                        column: x => x.SUBFK,
                        principalTable: "subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnrolledSubjects_SUBFK",
                table: "EnrolledSubjects",
                column: "SUBFK");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_subjects_SubjectFK",
                table: "Teachers",
                column: "SubjectFK",
                principalTable: "subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_subjects_SubjectFK",
                table: "Teachers");

            migrationBuilder.DropTable(
                name: "EnrolledSubjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_subjects",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "deaf_mute",
                table: "subjects");

            migrationBuilder.RenameTable(
                name: "subjects",
                newName: "Subject");

            migrationBuilder.AlterColumn<string>(
                name: "SubjectName",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubjectId",
                table: "Students",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Subject",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Subject_Name",
                table: "Subject",
                column: "Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subject",
                table: "Subject",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SubjectId",
                table: "Students",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Subject_SubjectId",
                table: "Students",
                column: "SubjectId",
                principalTable: "Subject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Subject_SubjectFK",
                table: "Teachers",
                column: "SubjectFK",
                principalTable: "Subject",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
