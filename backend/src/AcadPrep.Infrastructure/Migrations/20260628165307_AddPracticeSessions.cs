using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadPrep.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPracticeSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuestionGroupId",
                table: "QUESTIONS",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionType",
                table: "QUESTIONS",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TopicTag",
                table: "QUESTIONS",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PARTS",
                columns: table => new
                {
                    PartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    PartNumber = table.Column<int>(type: "int", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PARTS", x => x.PartId);
                    table.ForeignKey(
                        name: "FK_PARTS_EXAMS_ExamId",
                        column: x => x.ExamId,
                        principalTable: "EXAMS",
                        principalColumn: "ExamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PRACTICE_SESSIONS",
                columns: table => new
                {
                    PracticeSessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    SelectedParts = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SelectedTags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TimeLimit = table.Column<int>(type: "int", nullable: true),
                    CombinedQuestionsList = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRACTICE_SESSIONS", x => x.PracticeSessionId);
                    table.ForeignKey(
                        name: "FK_PRACTICE_SESSIONS_EXAMS_ExamId",
                        column: x => x.ExamId,
                        principalTable: "EXAMS",
                        principalColumn: "ExamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRACTICE_SESSIONS_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QUESTION_GROUPS",
                columns: table => new
                {
                    QuestionGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QUESTION_GROUPS", x => x.QuestionGroupId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QUESTIONS_QuestionGroupId",
                table: "QUESTIONS",
                column: "QuestionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PARTS_ExamId",
                table: "PARTS",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_PRACTICE_SESSIONS_ExamId",
                table: "PRACTICE_SESSIONS",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_PRACTICE_SESSIONS_UserId",
                table: "PRACTICE_SESSIONS",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_QUESTIONS_QUESTION_GROUPS_QuestionGroupId",
                table: "QUESTIONS",
                column: "QuestionGroupId",
                principalTable: "QUESTION_GROUPS",
                principalColumn: "QuestionGroupId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QUESTIONS_QUESTION_GROUPS_QuestionGroupId",
                table: "QUESTIONS");

            migrationBuilder.DropTable(
                name: "PARTS");

            migrationBuilder.DropTable(
                name: "PRACTICE_SESSIONS");

            migrationBuilder.DropTable(
                name: "QUESTION_GROUPS");

            migrationBuilder.DropIndex(
                name: "IX_QUESTIONS_QuestionGroupId",
                table: "QUESTIONS");

            migrationBuilder.DropColumn(
                name: "QuestionGroupId",
                table: "QUESTIONS");

            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "QUESTIONS");

            migrationBuilder.DropColumn(
                name: "TopicTag",
                table: "QUESTIONS");
        }
    }
}
