using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestManage.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentAccountArchival : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvestmentAccounts_UserId_Name",
                table: "InvestmentAccounts");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "InvestmentAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentAccounts_UserId_IsArchived_Name",
                table: "InvestmentAccounts",
                columns: new[] { "UserId", "IsArchived", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvestmentAccounts_UserId_IsArchived_Name",
                table: "InvestmentAccounts");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "InvestmentAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentAccounts_UserId_Name",
                table: "InvestmentAccounts",
                columns: new[] { "UserId", "Name" });
        }
    }
}
