using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace grad.Migrations
{
    /// <inheritdoc />
    public partial class v5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_subjects_SubjectFK",
                table: "Teachers");

            migrationBuilder.RenameColumn(
                name: "SubjectFK",
                table: "Teachers",
                newName: "SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Teachers_SubjectFK",
                table: "Teachers",
                newName: "IX_Teachers_SubjectId");

            migrationBuilder.CreateTable(
                name: "AssignedSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Assigned_At = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignedSubjects", x => x.Id);
                    table.UniqueConstraint("AK_AssignedSubjects_TeacherId_SubjectId", x => new { x.TeacherId, x.SubjectId });
                    table.ForeignKey(
                        name: "FK_AssignedSubjects_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignedSubjects_subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignedSubjects_SubjectId",
                table: "AssignedSubjects",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_subjects_SubjectId",
                table: "Teachers",
                column: "SubjectId",
                principalTable: "subjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_subjects_SubjectId",
                table: "Teachers");

            migrationBuilder.DropTable(
                name: "AssignedSubjects");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "Teachers",
                newName: "SubjectFK");

            migrationBuilder.RenameIndex(
                name: "IX_Teachers_SubjectId",
                table: "Teachers",
                newName: "IX_Teachers_SubjectFK");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_subjects_SubjectFK",
                table: "Teachers",
                column: "SubjectFK",
                principalTable: "subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
