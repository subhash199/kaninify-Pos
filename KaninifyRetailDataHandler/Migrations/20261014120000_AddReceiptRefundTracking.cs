using EntityFrameworkDatabaseLibrary.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    public partial class AddReceiptRefundTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>("Receipt_Refund_Reference", "SalesTransactions",
                type: "character varying(40)", maxLength: 40, nullable: true);
            migrationBuilder.AddColumn<DateTime>("Last_Refunded_At", "SalesTransactions",
                type: "timestamp with time zone", nullable: true);
            migrationBuilder.CreateIndex("IX_SalesTransactions_Receipt_Refund_Reference",
                "SalesTransactions", "Receipt_Refund_Reference", unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_SalesTransactions_Receipt_Refund_Reference", "SalesTransactions");
            migrationBuilder.DropColumn("Receipt_Refund_Reference", "SalesTransactions");
            migrationBuilder.DropColumn("Last_Refunded_At", "SalesTransactions");
        }
    }
}
