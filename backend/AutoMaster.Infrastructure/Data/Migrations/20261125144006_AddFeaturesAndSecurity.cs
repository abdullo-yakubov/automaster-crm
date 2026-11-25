using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMaster.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeaturesAndSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Refresh-токены теперь хранятся как SHA-256: старые открытые значения удаляем (пользователи просто перелогинятся).
            migrationBuilder.Sql("UPDATE \"Users\" SET \"RefreshToken\" = NULL, \"RefreshTokenExpiresAt\" = NULL;");

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndsAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MarketingCampaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Channel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Segment = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RecipientsCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromoCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DiscountPercent = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateOnly>(type: "date", nullable: false),
                    UsedCount = table.Column<int>(type: "integer", nullable: false),
                    MaxUses = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceBoxes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Specialization = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccupiedSince = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBoxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceBoxes_Orders_CurrentOrderId",
                        column: x => x.CurrentOrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserDashboards",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Widgets = table.Column<List<string>>(type: "text[]", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDashboards", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "VoiceCommandLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Transcript = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Intent = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Result = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceCommandLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RefreshToken",
                table: "Users",
                column: "RefreshToken");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId",
                table: "Customers",
                column: "UserId",
                unique: true,
                filter: "\"UserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_CreatedAt",
                table: "MarketingCampaigns",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodes_Code",
                table: "PromoCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBoxes_CurrentOrderId",
                table: "ServiceBoxes",
                column: "CurrentOrderId",
                unique: true,
                filter: "\"CurrentOrderId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCommandLogs_UserId_CreatedAt",
                table: "VoiceCommandLogs",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_UserId",
                table: "Customers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Users_UserId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "MarketingCampaigns");

            migrationBuilder.DropTable(
                name: "PromoCodes");

            migrationBuilder.DropTable(
                name: "ServiceBoxes");

            migrationBuilder.DropTable(
                name: "UserDashboards");

            migrationBuilder.DropTable(
                name: "VoiceCommandLogs");

            migrationBuilder.DropIndex(
                name: "IX_Users_RefreshToken",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Customers_UserId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockoutEndsAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Customers");
        }
    }
}
