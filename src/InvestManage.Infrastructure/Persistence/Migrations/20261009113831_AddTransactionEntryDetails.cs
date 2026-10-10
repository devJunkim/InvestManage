using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestManage.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionEntryDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_AccountInvestmentId_TradeDate",
                table: "Transactions");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "Transactions",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Transactions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountInvestmentId_TradeDate_CreatedAtUtc_Id",
                table: "Transactions",
                columns: new[] { "AccountInvestmentId", "TradeDate", "CreatedAtUtc", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_AccountInvestmentId_TradeDate_CreatedAtUtc_Id",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountInvestmentId_TradeDate",
                table: "Transactions",
                columns: new[] { "AccountInvestmentId", "TradeDate" });
        }
    }
}
