using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorChangeLogForAiObservability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_SKU",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "ChangeLogs");

            migrationBuilder.AddColumn<string>(
                name: "AiSummary",
                table: "ChangeLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AiSummaryGeneratedAt",
                table: "ChangeLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiSummaryStatus",
                table: "ChangeLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "NotRequested");

            migrationBuilder.AddColumn<Guid>(
                name: "CorrelationId",
                table: "ChangeLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ChangeLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogSource",
                table: "ChangeLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Business");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogs_AiSummaryStatus",
                table: "ChangeLogs",
                column: "AiSummaryStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogs_CorrelationId",
                table: "ChangeLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogs_LogSource",
                table: "ChangeLogs",
                column: "LogSource");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_SKU",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ChangeLogs_AiSummaryStatus",
                table: "ChangeLogs");

            migrationBuilder.DropIndex(
                name: "IX_ChangeLogs_CorrelationId",
                table: "ChangeLogs");

            migrationBuilder.DropIndex(
                name: "IX_ChangeLogs_LogSource",
                table: "ChangeLogs");

            migrationBuilder.DropColumn(
                name: "AiSummary",
                table: "ChangeLogs");

            migrationBuilder.DropColumn(
                name: "AiSummaryGeneratedAt",
                table: "ChangeLogs");

            migrationBuilder.DropColumn(
                name: "AiSummaryStatus",
                table: "ChangeLogs");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "ChangeLogs");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ChangeLogs");

            migrationBuilder.DropColumn(
                name: "LogSource",
                table: "ChangeLogs");

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "ChangeLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);
        }
    }
}
