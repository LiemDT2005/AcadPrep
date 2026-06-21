using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadPrep.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAchievementConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNotified",
                table: "UserAchievements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ConditionType",
                table: "Achievements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ConditionValue",
                table: "Achievements",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNotified",
                table: "UserAchievements");

            migrationBuilder.DropColumn(
                name: "ConditionType",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "ConditionValue",
                table: "Achievements");
        }
    }
}
