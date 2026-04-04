using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grad.Infrastructure.persistence.Migrations
{
    /// <inheritdoc />
    public partial class submissionV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectFK = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LevelFK = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Passed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_submissions_Students_SubmittedBy",
                        column: x => x.SubmittedBy,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_submissions_subjects_SubjectFK",
                        column: x => x.SubjectFK,
                        principalTable: "subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_submissions_SubjectFK",
                table: "submissions",
                column: "SubjectFK");

            migrationBuilder.CreateIndex(
                name: "IX_submissions_SubmittedBy",
                table: "submissions",
                column: "SubmittedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "submissions");
        }
    }
}
