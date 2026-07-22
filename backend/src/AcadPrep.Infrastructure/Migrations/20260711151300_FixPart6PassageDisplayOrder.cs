using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadPrep.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPart6PassageDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH Part6Passages AS (
                    SELECT
                        p.PassageId,
                        ROW_NUMBER() OVER (
                            PARTITION BY p.ExamId
                            ORDER BY MIN(q.QuestionNumber)
                        ) AS NewDisplayOrder
                    FROM PASSAGES p
                    INNER JOIN QUESTIONS q ON q.PassageId = p.PassageId AND q.Part = 6
                    WHERE p.QuestionGroupId IS NULL
                    GROUP BY p.PassageId, p.ExamId
                )
                UPDATE p
                SET DisplayOrder = pp.NewDisplayOrder
                FROM PASSAGES p
                INNER JOIN Part6Passages pp ON p.PassageId = pp.PassageId;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE p
                SET DisplayOrder = 1
                FROM PASSAGES p
                INNER JOIN QUESTIONS q ON q.PassageId = p.PassageId AND q.Part = 6
                WHERE p.QuestionGroupId IS NULL;
                """);
        }
    }
}
