using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestManage.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentItemManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvestmentItems_NormalizedCode",
                table: "InvestmentItems");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "InvestmentItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "InvestmentItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PricePrecision",
                table: "InvestmentItems",
                type: "int",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentItems_IsArchived_NormalizedCode",
                table: "InvestmentItems",
                columns: new[] { "IsArchived", "NormalizedCode" });

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentItems_IsArchived_Type_CurrencyCode",
                table: "InvestmentItems",
                columns: new[] { "IsArchived", "Type", "CurrencyCode" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_InvestmentItems_PricePrecision",
                table: "InvestmentItems",
                sql: "[PricePrecision] BETWEEN 0 AND 8");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvestmentItems_IsArchived_NormalizedCode",
                table: "InvestmentItems");

            migrationBuilder.DropIndex(
                name: "IX_InvestmentItems_IsArchived_Type_CurrencyCode",
                table: "InvestmentItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InvestmentItems_PricePrecision",
                table: "InvestmentItems");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "InvestmentItems");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "InvestmentItems");

            migrationBuilder.DropColumn(
                name: "PricePrecision",
                table: "InvestmentItems");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentItems_NormalizedCode",
                table: "InvestmentItems",
                column: "NormalizedCode");
        }
    }
}
