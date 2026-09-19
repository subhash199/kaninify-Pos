using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesTransactionReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The migration may have been interrupted after PostgreSQL added a column but
            // before EF recorded it in __EFMigrationsHistory.  These statements make retrying
            // the migration safe while still applying every missing schema change.
            migrationBuilder.Sql("""
                ALTER TABLE "SalesTransactions"
                ADD COLUMN IF NOT EXISTS "Transaction_Reference" character varying(40);

                ALTER TABLE "PaymentTerminalSettings"
                ADD COLUMN IF NOT EXISTS "DeviceCode" character varying(100) NOT NULL DEFAULT '';

                ALTER TABLE "PaymentTerminalSettings"
                ADD COLUMN IF NOT EXISTS "Postcode" character varying(20);

                ALTER TABLE "PaymentTerminalSettings"
                ADD COLUMN IF NOT EXISTS "Store_Address_Line_1" character varying(200);

                ALTER TABLE "PaymentTerminalSettings"
                ADD COLUMN IF NOT EXISTS "Store_Address_Line_2" character varying(200);

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_SalesTransaction_TransactionReference"
                ON "SalesTransactions" ("Transaction_Reference");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesTransaction_TransactionReference",
                table: "SalesTransactions");

            migrationBuilder.DropColumn(
                name: "Transaction_Reference",
                table: "SalesTransactions");

            migrationBuilder.DropColumn(
                name: "DeviceCode",
                table: "PaymentTerminalSettings");

            migrationBuilder.DropColumn(
                name: "Postcode",
                table: "PaymentTerminalSettings");

            migrationBuilder.DropColumn(
                name: "Store_Address_Line_1",
                table: "PaymentTerminalSettings");

            migrationBuilder.DropColumn(
                name: "Store_Address_Line_2",
                table: "PaymentTerminalSettings");
        }
    }
}
