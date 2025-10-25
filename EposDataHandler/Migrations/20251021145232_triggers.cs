using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class triggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        CREATE OR REPLACE FUNCTION insert_into_unsynced_log()
        RETURNS TRIGGER AS $$
         BEGIN
           IF TG_OP IN ('INSERT', 'UPDATE') THEN
             IF NEW.""SyncStatus"" ='Pending' then
                 IF NOT EXISTS (SELECT 1 FROM ""UnSyncedLogs""
                                 WHERE ""TableName"" = TG_TABLE_NAME
                                 AND ""RecordId"" = NEW.""Id"") THEN
                      INSERT INTO ""UnSyncedLogs"" (""TableName"", ""RecordId"", ""SyncStatus"", ""CreatedDate"", ""LastModified"", ""Operation"",""SyncRetryCount"")
                      VALUES (TG_TABLE_NAME, NEW.""Id"",'Pending', NOW(), NOW(), TG_OP, 0 );
                    END IF;
                END IF;
           END IF; 
             RETURN NEW;
         END;
         $$ LANGUAGE plpgsql;
         ");

            migrationBuilder.Sql(@"
            CREATE TRIGGER product_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""Products""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");

            migrationBuilder.Sql(@"
            CREATE TRIGGER sites_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""Sites""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");

            migrationBuilder.Sql(@"
            CREATE TRIGGER daylog_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""DayLogs""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");


            migrationBuilder.Sql(@"
            CREATE TRIGGER payout_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""Payouts""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");


            migrationBuilder.Sql(@"
            CREATE TRIGGER posuser_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""PosUsers""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");


            migrationBuilder.Sql(@"
            CREATE TRIGGER promotion_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""Promotions""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");


            migrationBuilder.Sql(@"
            CREATE TRIGGER salesitemtransaction_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""SalesItemTransactions""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");


            migrationBuilder.Sql(@"
            CREATE TRIGGER salestransaction_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""SalesTransactions""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");


            migrationBuilder.Sql(@"
            CREATE TRIGGER suppliers_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""Suppliers""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");


            migrationBuilder.Sql(@"
            CREATE TRIGGER supplieritems_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""SupplierItems""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");


            migrationBuilder.Sql(@"
            CREATE TRIGGER tills_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""Tills""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");

            migrationBuilder.Sql(@"
            CREATE TRIGGER stocktransactions_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""StockTransactions""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");

            migrationBuilder.Sql(@"
            CREATE TRIGGER shifts_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""Shifts""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");
            migrationBuilder.Sql(@"
            CREATE TRIGGER usersiteaccesses_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""UserSiteAccesses""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");

            migrationBuilder.Sql(@"
            CREATE TRIGGER voidedproducts_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""VoidedProducts""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");

            migrationBuilder.Sql(@"
            CREATE TRIGGER receiptprinters_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""ReceiptPrinters""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");

            migrationBuilder.Sql(@"
            CREATE TRIGGER drawerlogs_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""DrawerLogs""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");

            migrationBuilder.Sql(@"
            CREATE TRIGGER stockrefills_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""StockRefills""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");

            migrationBuilder.Sql(@"
            CREATE TRIGGER errorlogs_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""ErrorLogs""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");

            migrationBuilder.Sql(@"
            CREATE TRIGGER unknownproducts_insert_into_unsynced_log_on_insert_update
            AFTER INSERT OR UPDATE ON ""UnknownProducts""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_unsynced_log();

            ");


        }

       

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS product_insert_into_unsynced_log_on_insert_update ON ""Products"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS sites_insert_into_unsynced_log_on_insert_update ON ""Sites"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS daylog_insert_into_unsynced_log_on_insert_update ON ""DayLogs"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS payout_insert_into_unsynced_log_on_insert_update ON ""Payouts"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS posuser_insert_into_unsynced_log_on_insert_update ON ""PosUsers"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS promotion_insert_into_unsynced_log_on_insert_update ON ""Promotions"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS salesitemtransaction_insert_into_unsynced_log_on_insert_update ON ""SalesItemTransactions"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS salestransaction_insert_into_unsynced_log_on_insert_update ON ""SalesTransactions"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS suppliers_insert_into_unsynced_log_on_insert_update ON ""Suppliers"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS supplieritems_insert_into_unsynced_log_on_insert_update ON ""SupplierItems"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS tills_insert_into_unsynced_log_on_insert_update ON ""Tills"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS stocktransactions_insert_into_unsynced_log_on_insert_update ON ""StockTransactions"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS shifts_insert_into_unsynced_log_on_insert_update ON ""Shifts"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS usersiteaccesses_insert_into_unsynced_log_on_insert_update ON ""UserSiteAccesses"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS voidedproducts_insert_into_unsynced_log_on_insert_update ON ""VoidedProducts"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS receiptprinters_insert_into_unsynced_log_on_insert_update ON ""ReceiptPrinters"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS drawerlogs_insert_into_unsynced_log_on_insert_update ON ""DrawerLogs"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS stockrefills_insert_into_unsynced_log_on_insert_update ON ""StockRefills"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS errorlogs_insert_into_unsynced_log_on_insert_update ON ""ErrorLogs"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS unknownproducts_insert_into_unsynced_log_on_insert_update ON ""UnknownProducts"";");
    
            // Drop the trigger function
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS insert_into_unsynced_log();");
          
        }
    }
}
