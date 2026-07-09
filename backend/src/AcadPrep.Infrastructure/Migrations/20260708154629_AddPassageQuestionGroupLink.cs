using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadPrep.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPassageQuestionGroupLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "PASSAGES",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "QuestionGroupId",
                table: "PASSAGES",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PASSAGES_QuestionGroupId_DisplayOrder",
                table: "PASSAGES",
                columns: new[] { "QuestionGroupId", "DisplayOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_PASSAGES_QUESTION_GROUPS_QuestionGroupId",
                table: "PASSAGES",
                column: "QuestionGroupId",
                principalTable: "QUESTION_GROUPS",
                principalColumn: "QuestionGroupId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PASSAGES_QUESTION_GROUPS_QuestionGroupId",
                table: "PASSAGES");

            migrationBuilder.DropIndex(
                name: "IX_PASSAGES_QuestionGroupId_DisplayOrder",
                table: "PASSAGES");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "PASSAGES");

            migrationBuilder.DropColumn(
                name: "QuestionGroupId",
                table: "PASSAGES");
        }
    }
}
