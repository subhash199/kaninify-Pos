using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddCardTransactionResponseDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "CardTransactions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Provider_Created_At",
                table: "CardTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Provider_Updated_At",
                table: "CardTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Requested_Amount_Minor_Units",
                table: "CardTransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Requested_Tip_Minor_Units",
                table: "CardTransactions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "CardTransactions");

            migrationBuilder.DropColumn(
                name: "Provider_Created_At",
                table: "CardTransactions");

            migrationBuilder.DropColumn(
                name: "Provider_Updated_At",
                table: "CardTransactions");

            migrationBuilder.DropColumn(
                name: "Requested_Amount_Minor_Units",
                table: "CardTransactions");

            migrationBuilder.DropColumn(
                name: "Requested_Tip_Minor_Units",
                table: "CardTransactions");
        }
    }
}
