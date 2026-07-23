using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadPrep.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingVNPay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PAYMENT_WEBHOOK_LOGS",
                columns: table => new
                {
                    WebhookLogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Provider = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    OrderCode = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignatureValid = table.Column<bool>(type: "bit", nullable: false),
                    Processed = table.Column<bool>(type: "bit", nullable: false),
                    ProcessResult = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAYMENT_WEBHOOK_LOGS", x => x.WebhookLogId);
                });

            migrationBuilder.CreateTable(
                name: "PLANS",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PriceVnd = table.Column<long>(type: "bigint", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PLANS", x => x.PlanId);
                });

            migrationBuilder.CreateTable(
                name: "ORDERS",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    OrderCode = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    AmountVnd = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    PaymentProvider = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ProviderTxnId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ProviderResponseCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    ProviderBankCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ProviderRawPayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDERS", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_ORDERS_PLANS_PlanId",
                        column: x => x.PlanId,
                        principalTable: "PLANS",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ORDERS_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "USER_SUBSCRIPTIONS",
                columns: table => new
                {
                    SubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    SourceOrderId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_SUBSCRIPTIONS", x => x.SubscriptionId);
                    table.ForeignKey(
                        name: "FK_USER_SUBSCRIPTIONS_ORDERS_SourceOrderId",
                        column: x => x.SourceOrderId,
                        principalTable: "ORDERS",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_USER_SUBSCRIPTIONS_PLANS_PlanId",
                        column: x => x.PlanId,
                        principalTable: "PLANS",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_USER_SUBSCRIPTIONS_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_CreatedAt",
                table: "ORDERS",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_OrderCode",
                table: "ORDERS",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_PlanId",
                table: "ORDERS",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_UserId_Status",
                table: "ORDERS",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_WEBHOOK_LOGS_OrderCode",
                table: "PAYMENT_WEBHOOK_LOGS",
                column: "OrderCode");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_WEBHOOK_LOGS_ReceivedAt",
                table: "PAYMENT_WEBHOOK_LOGS",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PLANS_Code",
                table: "PLANS",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_SUBSCRIPTIONS_PlanId",
                table: "USER_SUBSCRIPTIONS",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_USER_SUBSCRIPTIONS_SourceOrderId",
                table: "USER_SUBSCRIPTIONS",
                column: "SourceOrderId",
                unique: true,
                filter: "[SourceOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_USER_SUBSCRIPTIONS_UserId_Status_ExpiresAt",
                table: "USER_SUBSCRIPTIONS",
                columns: new[] { "UserId", "Status", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PAYMENT_WEBHOOK_LOGS");

            migrationBuilder.DropTable(
                name: "USER_SUBSCRIPTIONS");

            migrationBuilder.DropTable(
                name: "ORDERS");

            migrationBuilder.DropTable(
                name: "PLANS");
        }
    }
}
