using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadPrep.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExamQuestionMediaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AudioEndSecond",
                table: "QUESTIONS",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AudioStartSecond",
                table: "QUESTIONS",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "QUESTIONS",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AudioEndSecond",
                table: "QUESTION_GROUPS",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AudioStartSecond",
                table: "QUESTION_GROUPS",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "QUESTION_GROUPS",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExamId",
                table: "QUESTION_GROUPS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "QUESTION_GROUPS",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "PASSAGES",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "PASSAGES",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "EXAMS",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "EXAMS",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.CreateIndex(
                name: "IX_QUESTION_GROUPS_ExamId",
                table: "QUESTION_GROUPS",
                column: "ExamId");

            migrationBuilder.AddForeignKey(
                name: "FK_QUESTION_GROUPS_EXAMS_ExamId",
                table: "QUESTION_GROUPS",
                column: "ExamId",
                principalTable: "EXAMS",
                principalColumn: "ExamId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QUESTION_GROUPS_EXAMS_ExamId",
                table: "QUESTION_GROUPS");

            migrationBuilder.DropIndex(
                name: "IX_QUESTION_GROUPS_ExamId",
                table: "QUESTION_GROUPS");

            migrationBuilder.DropColumn(
                name: "AudioEndSecond",
                table: "QUESTIONS");

            migrationBuilder.DropColumn(
                name: "AudioStartSecond",
                table: "QUESTIONS");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "QUESTIONS");

            migrationBuilder.DropColumn(
                name: "AudioEndSecond",
                table: "QUESTION_GROUPS");

            migrationBuilder.DropColumn(
                name: "AudioStartSecond",
                table: "QUESTION_GROUPS");

            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "QUESTION_GROUPS");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "QUESTION_GROUPS");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "QUESTION_GROUPS");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "PASSAGES");

            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "EXAMS");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EXAMS");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "PASSAGES",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
