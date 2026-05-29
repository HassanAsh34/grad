using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class inquery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inquery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepliedToId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    submitterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmitterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    QuestionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Submitted_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Resovled_AT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inquery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inquery_users_submitterId",
                        column: x => x.submitterId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inquery_submitterId",
                table: "Inquery",
                column: "submitterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inquery");
        }
    }
}
