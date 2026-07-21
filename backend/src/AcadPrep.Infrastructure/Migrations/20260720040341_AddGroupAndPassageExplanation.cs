using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadPrep.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupAndPassageExplanation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Explanation",
                table: "QUESTION_GROUPS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Explanation",
                table: "PASSAGES",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Explanation",
                table: "QUESTION_GROUPS");

            migrationBuilder.DropColumn(
                name: "Explanation",
                table: "PASSAGES");
        }
    }
}
