using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadPrep.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EXAMS",
                columns: table => new
                {
                    ExamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXAMS", x => x.ExamId);
                });

            migrationBuilder.CreateTable(
                name: "ROLES",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLES", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "VOCABULARIES",
                columns: table => new
                {
                    VocabularyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Word = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phonetic = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Meaning = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Example = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VOCABULARIES", x => x.VocabularyId);
                });

            migrationBuilder.CreateTable(
                name: "PASSAGES",
                columns: table => new
                {
                    PassageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PASSAGES", x => x.PassageId);
                    table.ForeignKey(
                        name: "FK_PASSAGES_EXAMS_ExamId",
                        column: x => x.ExamId,
                        principalTable: "EXAMS",
                        principalColumn: "ExamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "Active"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.UserId);
                    table.CheckConstraint("CHK_UserStatus", "[Status] IN ('Active', 'Inactive')");
                    table.ForeignKey(
                        name: "FK_USERS_ROLES_RoleId",
                        column: x => x.RoleId,
                        principalTable: "ROLES",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VOCAB_PASSAGES",
                columns: table => new
                {
                    VocabPassageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VocabularyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VOCAB_PASSAGES", x => x.VocabPassageId);
                    table.ForeignKey(
                        name: "FK_VOCAB_PASSAGES_VOCABULARIES_VocabularyId",
                        column: x => x.VocabularyId,
                        principalTable: "VOCABULARIES",
                        principalColumn: "VocabularyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QUESTIONS",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionNumber = table.Column<int>(type: "int", nullable: false),
                    Part = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AudioUrl = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    CorrectOption = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    PassageId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QUESTIONS", x => x.QuestionId);
                    table.CheckConstraint("CHK_CorrectOption", "[CorrectOption] IN ('A', 'B', 'C', 'D')");
                    table.CheckConstraint("CHK_QuestionPart", "[Part] BETWEEN 1 AND 7");
                    table.ForeignKey(
                        name: "FK_QUESTIONS_EXAMS_ExamId",
                        column: x => x.ExamId,
                        principalTable: "EXAMS",
                        principalColumn: "ExamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QUESTIONS_PASSAGES_PassageId",
                        column: x => x.PassageId,
                        principalTable: "PASSAGES",
                        principalColumn: "PassageId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AUDITLOGS",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TableAffected = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUDITLOGS", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_AUDITLOGS_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EXAM_ATTEMPTS",
                columns: table => new
                {
                    AttemptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    ListeningScore = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReadingScore = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalScore = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RemainingTime = table.Column<int>(type: "int", nullable: false),
                    IsSubmitted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXAM_ATTEMPTS", x => x.AttemptId);
                    table.ForeignKey(
                        name: "FK_EXAM_ATTEMPTS_EXAMS_ExamId",
                        column: x => x.ExamId,
                        principalTable: "EXAMS",
                        principalColumn: "ExamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EXAM_ATTEMPTS_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SAVED_VOCABULARIES",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VocabularyId = table.Column<int>(type: "int", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DateSaved = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SAVED_VOCABULARIES", x => new { x.UserId, x.VocabularyId });
                    table.ForeignKey(
                        name: "FK_SAVED_VOCABULARIES_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SAVED_VOCABULARIES_VOCABULARIES_VocabularyId",
                        column: x => x.VocabularyId,
                        principalTable: "VOCABULARIES",
                        principalColumn: "VocabularyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STUDY_STREAKS",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxStreak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastActiveDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STUDY_STREAKS", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_STUDY_STREAKS_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QUESTION_OPTIONS",
                columns: table => new
                {
                    OptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    OptionLetter = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QUESTION_OPTIONS", x => x.OptionId);
                    table.CheckConstraint("CHK_OptionLetter", "[OptionLetter] IN ('A', 'B', 'C', 'D')");
                    table.ForeignKey(
                        name: "FK_QUESTION_OPTIONS_QUESTIONS_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "QUESTIONS",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ATTEMPT_ANSWERS",
                columns: table => new
                {
                    AttemptId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    SelectedOption = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ATTEMPT_ANSWERS", x => new { x.AttemptId, x.QuestionId });
                    table.CheckConstraint("CHK_SelectedOption", "[SelectedOption] IN ('A', 'B', 'C', 'D')");
                    table.ForeignKey(
                        name: "FK_ATTEMPT_ANSWERS_EXAM_ATTEMPTS_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "EXAM_ATTEMPTS",
                        principalColumn: "AttemptId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ATTEMPT_ANSWERS_QUESTIONS_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "QUESTIONS",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ATTEMPT_ANSWERS_QuestionId",
                table: "ATTEMPT_ANSWERS",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AUDITLOGS_UserId",
                table: "AUDITLOGS",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EXAM_ATTEMPTS_ExamId",
                table: "EXAM_ATTEMPTS",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_EXAM_ATTEMPTS_UserId",
                table: "EXAM_ATTEMPTS",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PASSAGES_ExamId",
                table: "PASSAGES",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_QUESTION_OPTIONS_QuestionId",
                table: "QUESTION_OPTIONS",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QUESTIONS_ExamId",
                table: "QUESTIONS",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_QUESTIONS_PassageId",
                table: "QUESTIONS",
                column: "PassageId");

            migrationBuilder.CreateIndex(
                name: "IX_ROLES_RoleName",
                table: "ROLES",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SAVED_VOCABULARIES_VocabularyId",
                table: "SAVED_VOCABULARIES",
                column: "VocabularyId");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_Email",
                table: "USERS",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_RoleId",
                table: "USERS",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_VOCAB_PASSAGES_VocabularyId",
                table: "VOCAB_PASSAGES",
                column: "VocabularyId");

            migrationBuilder.CreateIndex(
                name: "IX_VOCABULARIES_Word",
                table: "VOCABULARIES",
                column: "Word",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ATTEMPT_ANSWERS");

            migrationBuilder.DropTable(
                name: "AUDITLOGS");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "QUESTION_OPTIONS");

            migrationBuilder.DropTable(
                name: "SAVED_VOCABULARIES");

            migrationBuilder.DropTable(
                name: "STUDY_STREAKS");

            migrationBuilder.DropTable(
                name: "VOCAB_PASSAGES");

            migrationBuilder.DropTable(
                name: "EXAM_ATTEMPTS");

            migrationBuilder.DropTable(
                name: "QUESTIONS");

            migrationBuilder.DropTable(
                name: "VOCABULARIES");

            migrationBuilder.DropTable(
                name: "USERS");

            migrationBuilder.DropTable(
                name: "PASSAGES");

            migrationBuilder.DropTable(
                name: "ROLES");

            migrationBuilder.DropTable(
                name: "EXAMS");
        }
    }
}
