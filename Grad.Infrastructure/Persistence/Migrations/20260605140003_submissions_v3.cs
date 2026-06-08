using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class submissions_v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Progress",
                table: "Enrollents",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Progress",
                table: "Enrollents");
        }
    }
}
