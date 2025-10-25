using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class unsyncedlogssynced : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        CREATE OR REPLACE FUNCTION insert_into_synced_logs_from_unsynced_logs()
                RETURNS TRIGGER AS $$
                 BEGIN
                    IF TG_OP = 'UPDATE' THEN
                        IF NEW.""SyncStatus"" = 'Synced' THEN
                            INSERT INTO ""SyncedLogs"" (""TableName"", ""RecordId"", ""SyncStatus"", ""Operation"", ""SyncLocation"", ""SyncedDateTime"", ""LastModified"")
                            VALUES (NEW.""TableName"", NEW.""Id"", 'Synced', TG_OP, 'Central', NOW(), NOW());    
                            DELETE FROM ""UnSyncedLogs""
                            WHERE ""TableName"" = NEW.""TableName""
                                AND ""RecordId"" = NEW.""RecordId"";
                        END IF;
                    END IF;
                    RETURN NEW;
                END;
         $$ LANGUAGE plpgsql;
            
            ");

            migrationBuilder.Sql(@"
        CREATE TRIGGER trg_insert_into_synced_logs_from_unsynced_logs
            AFTER UPDATE ON ""UnSyncedLogs""
            FOR EACH ROW
            EXECUTE FUNCTION insert_into_synced_logs_from_unsynced_logs();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_insert_into_synced_logs_from_unsynced_logs ON ""UnSyncedLogs"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS insert_into_synced_logs_from_unsynced_logs();");
        }
    }
}
