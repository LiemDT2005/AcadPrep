using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadPrep.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExamSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExamSeriesId",
                table: "EXAMS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EXAM_SERIES",
                columns: table => new
                {
                    ExamSeriesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXAM_SERIES", x => x.ExamSeriesId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EXAMS_ExamSeriesId",
                table: "EXAMS",
                column: "ExamSeriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_EXAMS_EXAM_SERIES_ExamSeriesId",
                table: "EXAMS",
                column: "ExamSeriesId",
                principalTable: "EXAM_SERIES",
                principalColumn: "ExamSeriesId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EXAMS_EXAM_SERIES_ExamSeriesId",
                table: "EXAMS");

            migrationBuilder.DropTable(
                name: "EXAM_SERIES");

            migrationBuilder.DropIndex(
                name: "IX_EXAMS_ExamSeriesId",
                table: "EXAMS");

            migrationBuilder.DropColumn(
                name: "ExamSeriesId",
                table: "EXAMS");
        }
    }
}
