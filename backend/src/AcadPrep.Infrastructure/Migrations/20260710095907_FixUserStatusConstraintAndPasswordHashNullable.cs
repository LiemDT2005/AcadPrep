using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadPrep.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserStatusConstraintAndPasswordHashNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE [USERS] DROP CONSTRAINT [CHK_UserStatus];
                ALTER TABLE [USERS] ALTER COLUMN [PasswordHash] varchar(255) NULL;
                ALTER TABLE [USERS] ADD CONSTRAINT [CHK_UserStatus] 
                    CHECK ([Status] IN ('Active', 'Inactive', 'Suspended'));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE [USERS] DROP CONSTRAINT [CHK_UserStatus];
                ALTER TABLE [USERS] ALTER COLUMN [PasswordHash] varchar(255) NOT NULL;
                ALTER TABLE [USERS] ADD CONSTRAINT [CHK_UserStatus] 
                    CHECK ([Status] IN ('Active', 'Inactive'));
            ");
        }
    }
}
