using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class SalesItemWeightDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price_Per_Kg",
                table: "SalesItemTransactions",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight_Kg",
                table: "SalesItemTransactions",
                type: "numeric(18,3)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price_Per_Kg",
                table: "SalesItemTransactions");

            migrationBuilder.DropColumn(
                name: "Weight_Kg",
                table: "SalesItemTransactions");
        }
    }
}
