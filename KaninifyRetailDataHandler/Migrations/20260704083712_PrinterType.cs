using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class PrinterType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Printer_Type",
                table: "ReceiptPrinters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Printer_Type",
                table: "ReceiptPrinters");
        }
    }
}
