using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadPrep.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPracticeSessionScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnswersJson",
                table: "PRACTICE_SESSIONS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "PRACTICE_SESSIONS",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CorrectCount",
                table: "PRACTICE_SESSIONS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubmitted",
                table: "PRACTICE_SESSIONS",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ListeningCorrect",
                table: "PRACTICE_SESSIONS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ListeningTotal",
                table: "PRACTICE_SESSIONS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReadingCorrect",
                table: "PRACTICE_SESSIONS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReadingTotal",
                table: "PRACTICE_SESSIONS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalQuestions",
                table: "PRACTICE_SESSIONS",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswersJson",
                table: "PRACTICE_SESSIONS");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "PRACTICE_SESSIONS");

            migrationBuilder.DropColumn(
                name: "CorrectCount",
                table: "PRACTICE_SESSIONS");

            migrationBuilder.DropColumn(
                name: "IsSubmitted",
                table: "PRACTICE_SESSIONS");

            migrationBuilder.DropColumn(
                name: "ListeningCorrect",
                table: "PRACTICE_SESSIONS");

            migrationBuilder.DropColumn(
                name: "ListeningTotal",
                table: "PRACTICE_SESSIONS");

            migrationBuilder.DropColumn(
                name: "ReadingCorrect",
                table: "PRACTICE_SESSIONS");

            migrationBuilder.DropColumn(
                name: "ReadingTotal",
                table: "PRACTICE_SESSIONS");

            migrationBuilder.DropColumn(
                name: "TotalQuestions",
                table: "PRACTICE_SESSIONS");
        }
    }
}
