using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class cascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_DayLogs_DayLogId",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_PosUsers_Created_By_Id",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_PosUsers_Last_Modified_By_Id",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_Shifts_Shift_Id",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_Sites_Site_Id",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_Suppliers_SupplierId",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_Tills_Till_Id",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DrawerLogs_PosUsers_Created_By_Id",
                table: "DrawerLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DrawerLogs_PosUsers_Last_Modified_By_Id",
                table: "DrawerLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DrawerLogs_PosUsers_OpenedById",
                table: "DrawerLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DrawerLogs_Sites_Site_Id",
                table: "DrawerLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DrawerLogs_Tills_Till_Id",
                table: "DrawerLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ErrorLogs_PosUsers_Resolved_By_Id",
                table: "ErrorLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ErrorLogs_PosUsers_User_Id",
                table: "ErrorLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ErrorLogs_Sites_Site_Id",
                table: "ErrorLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ErrorLogs_Tills_Till_Id",
                table: "ErrorLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PosUsers_Sites_Primary_Site_Id",
                table: "PosUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_PosUsers_Sites_Site_Id",
                table: "PosUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_PosUsers_Tills_Till_Id",
                table: "PosUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_PosUsers_Created_By_Id",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_PosUsers_Last_Modified_By_Id",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Promotions_Promotion_Id",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Sites_Site_Id",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Tills_Till_Id",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesItemTransactions_Payouts_SalesPayout_ID",
                table: "SalesItemTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesItemTransactions_PosUsers_Created_By_Id",
                table: "SalesItemTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesItemTransactions_PosUsers_Last_Modified_By_Id",
                table: "SalesItemTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesItemTransactions_Products_Product_ID",
                table: "SalesItemTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesItemTransactions_Promotions_Promotion_ID",
                table: "SalesItemTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesTransactions_PosUsers_Created_By_Id",
                table: "SalesTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesTransactions_PosUsers_Last_Modified_By_Id",
                table: "SalesTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesTransactions_Sites_Site_Id",
                table: "SalesTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesTransactions_Tills_Till_Id",
                table: "SalesTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_PosUsers_PosUser_Id",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Sites_Site_Id",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Tills_Till_Id",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_PosUsers_Created_By_Id",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_PosUsers_Last_Modified_By_Id",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_StockRefills_PosUsers_Created_By_ID",
                table: "StockRefills");

            migrationBuilder.DropForeignKey(
                name: "FK_StockRefills_PosUsers_Last_Modified_By_ID",
                table: "StockRefills");

            migrationBuilder.DropForeignKey(
                name: "FK_StockRefills_PosUsers_Refilled_By",
                table: "StockRefills");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_PosUsers_Created_By_Id",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_PosUsers_Last_Modified_By_Id",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Products_ProductId",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Sites_From_Site_Id",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Sites_To_Site_Id",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Tills_Till_Id",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_UnknownProducts_DayLogs_DaylogId",
                table: "UnknownProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_UnknownProducts_PosUsers_CreatedById",
                table: "UnknownProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_UnknownProducts_Shifts_ShiftId",
                table: "UnknownProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_UnknownProducts_Sites_SiteId",
                table: "UnknownProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_UnknownProducts_Tills_TillId",
                table: "UnknownProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_PosUsers_Created_By_Id",
                table: "Vouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_PosUsers_Last_Modified_By_Id",
                table: "Vouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_Sites_Site_Id",
                table: "Vouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_Tills_Till_Id",
                table: "Vouchers");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_DayLogs_DayLogId",
                table: "DeliveryInvoices",
                column: "DayLogId",
                principalTable: "DayLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_PosUsers_Created_By_Id",
                table: "DeliveryInvoices",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_PosUsers_Last_Modified_By_Id",
                table: "DeliveryInvoices",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_Shifts_Shift_Id",
                table: "DeliveryInvoices",
                column: "Shift_Id",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_Sites_Site_Id",
                table: "DeliveryInvoices",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_Suppliers_SupplierId",
                table: "DeliveryInvoices",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_Tills_Till_Id",
                table: "DeliveryInvoices",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_PosUsers_Created_By_Id",
                table: "DrawerLogs",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_PosUsers_Last_Modified_By_Id",
                table: "DrawerLogs",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_PosUsers_OpenedById",
                table: "DrawerLogs",
                column: "OpenedById",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_Sites_Site_Id",
                table: "DrawerLogs",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_Tills_Till_Id",
                table: "DrawerLogs",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorLogs_PosUsers_Resolved_By_Id",
                table: "ErrorLogs",
                column: "Resolved_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorLogs_PosUsers_User_Id",
                table: "ErrorLogs",
                column: "User_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorLogs_Sites_Site_Id",
                table: "ErrorLogs",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorLogs_Tills_Till_Id",
                table: "ErrorLogs",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PosUsers_Sites_Primary_Site_Id",
                table: "PosUsers",
                column: "Primary_Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PosUsers_Sites_Site_Id",
                table: "PosUsers",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PosUsers_Tills_Till_Id",
                table: "PosUsers",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PosUsers_Created_By_Id",
                table: "Products",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PosUsers_Last_Modified_By_Id",
                table: "Products",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Promotions_Promotion_Id",
                table: "Products",
                column: "Promotion_Id",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Sites_Site_Id",
                table: "Products",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Tills_Till_Id",
                table: "Products",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesItemTransactions_Payouts_SalesPayout_ID",
                table: "SalesItemTransactions",
                column: "SalesPayout_ID",
                principalTable: "Payouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesItemTransactions_PosUsers_Created_By_Id",
                table: "SalesItemTransactions",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesItemTransactions_PosUsers_Last_Modified_By_Id",
                table: "SalesItemTransactions",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesItemTransactions_Products_Product_ID",
                table: "SalesItemTransactions",
                column: "Product_ID",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesItemTransactions_Promotions_Promotion_ID",
                table: "SalesItemTransactions",
                column: "Promotion_ID",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesTransactions_PosUsers_Created_By_Id",
                table: "SalesTransactions",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesTransactions_PosUsers_Last_Modified_By_Id",
                table: "SalesTransactions",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesTransactions_Sites_Site_Id",
                table: "SalesTransactions",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesTransactions_Tills_Till_Id",
                table: "SalesTransactions",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_PosUsers_PosUser_Id",
                table: "Shifts",
                column: "PosUser_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Sites_Site_Id",
                table: "Shifts",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Tills_Till_Id",
                table: "Shifts",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_PosUsers_Created_By_Id",
                table: "Sites",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_PosUsers_Last_Modified_By_Id",
                table: "Sites",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockRefills_PosUsers_Created_By_ID",
                table: "StockRefills",
                column: "Created_By_ID",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockRefills_PosUsers_Last_Modified_By_ID",
                table: "StockRefills",
                column: "Last_Modified_By_ID",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockRefills_PosUsers_Refilled_By",
                table: "StockRefills",
                column: "Refilled_By",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_PosUsers_Created_By_Id",
                table: "StockTransactions",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_PosUsers_Last_Modified_By_Id",
                table: "StockTransactions",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Products_ProductId",
                table: "StockTransactions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Sites_From_Site_Id",
                table: "StockTransactions",
                column: "From_Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Sites_To_Site_Id",
                table: "StockTransactions",
                column: "To_Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Tills_Till_Id",
                table: "StockTransactions",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UnknownProducts_DayLogs_DaylogId",
                table: "UnknownProducts",
                column: "DaylogId",
                principalTable: "DayLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UnknownProducts_PosUsers_CreatedById",
                table: "UnknownProducts",
                column: "CreatedById",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UnknownProducts_Shifts_ShiftId",
                table: "UnknownProducts",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UnknownProducts_Sites_SiteId",
                table: "UnknownProducts",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UnknownProducts_Tills_TillId",
                table: "UnknownProducts",
                column: "TillId",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_PosUsers_Created_By_Id",
                table: "Vouchers",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_PosUsers_Last_Modified_By_Id",
                table: "Vouchers",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_Sites_Site_Id",
                table: "Vouchers",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_Tills_Till_Id",
                table: "Vouchers",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_DayLogs_DayLogId",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_PosUsers_Created_By_Id",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_PosUsers_Last_Modified_By_Id",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_Shifts_Shift_Id",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_Sites_Site_Id",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_Suppliers_SupplierId",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInvoices_Tills_Till_Id",
                table: "DeliveryInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_DrawerLogs_PosUsers_Created_By_Id",
                table: "DrawerLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DrawerLogs_PosUsers_Last_Modified_By_Id",
                table: "DrawerLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DrawerLogs_PosUsers_OpenedById",
                table: "DrawerLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DrawerLogs_Sites_Site_Id",
                table: "DrawerLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DrawerLogs_Tills_Till_Id",
                table: "DrawerLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ErrorLogs_PosUsers_Resolved_By_Id",
                table: "ErrorLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ErrorLogs_PosUsers_User_Id",
                table: "ErrorLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ErrorLogs_Sites_Site_Id",
                table: "ErrorLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ErrorLogs_Tills_Till_Id",
                table: "ErrorLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PosUsers_Sites_Primary_Site_Id",
                table: "PosUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_PosUsers_Sites_Site_Id",
                table: "PosUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_PosUsers_Tills_Till_Id",
                table: "PosUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_PosUsers_Created_By_Id",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_PosUsers_Last_Modified_By_Id",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Promotions_Promotion_Id",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Sites_Site_Id",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Tills_Till_Id",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesItemTransactions_Payouts_SalesPayout_ID",
                table: "SalesItemTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesItemTransactions_PosUsers_Created_By_Id",
                table: "SalesItemTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesItemTransactions_PosUsers_Last_Modified_By_Id",
                table: "SalesItemTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesItemTransactions_Products_Product_ID",
                table: "SalesItemTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesItemTransactions_Promotions_Promotion_ID",
                table: "SalesItemTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesTransactions_PosUsers_Created_By_Id",
                table: "SalesTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesTransactions_PosUsers_Last_Modified_By_Id",
                table: "SalesTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesTransactions_Sites_Site_Id",
                table: "SalesTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesTransactions_Tills_Till_Id",
                table: "SalesTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_PosUsers_PosUser_Id",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Sites_Site_Id",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Tills_Till_Id",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_PosUsers_Created_By_Id",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_PosUsers_Last_Modified_By_Id",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_StockRefills_PosUsers_Created_By_ID",
                table: "StockRefills");

            migrationBuilder.DropForeignKey(
                name: "FK_StockRefills_PosUsers_Last_Modified_By_ID",
                table: "StockRefills");

            migrationBuilder.DropForeignKey(
                name: "FK_StockRefills_PosUsers_Refilled_By",
                table: "StockRefills");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_PosUsers_Created_By_Id",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_PosUsers_Last_Modified_By_Id",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Products_ProductId",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Sites_From_Site_Id",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Sites_To_Site_Id",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Tills_Till_Id",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_UnknownProducts_DayLogs_DaylogId",
                table: "UnknownProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_UnknownProducts_PosUsers_CreatedById",
                table: "UnknownProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_UnknownProducts_Shifts_ShiftId",
                table: "UnknownProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_UnknownProducts_Sites_SiteId",
                table: "UnknownProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_UnknownProducts_Tills_TillId",
                table: "UnknownProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_PosUsers_Created_By_Id",
                table: "Vouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_PosUsers_Last_Modified_By_Id",
                table: "Vouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_Sites_Site_Id",
                table: "Vouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_Tills_Till_Id",
                table: "Vouchers");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_DayLogs_DayLogId",
                table: "DeliveryInvoices",
                column: "DayLogId",
                principalTable: "DayLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_PosUsers_Created_By_Id",
                table: "DeliveryInvoices",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_PosUsers_Last_Modified_By_Id",
                table: "DeliveryInvoices",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_Shifts_Shift_Id",
                table: "DeliveryInvoices",
                column: "Shift_Id",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_Sites_Site_Id",
                table: "DeliveryInvoices",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_Suppliers_SupplierId",
                table: "DeliveryInvoices",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInvoices_Tills_Till_Id",
                table: "DeliveryInvoices",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_PosUsers_Created_By_Id",
                table: "DrawerLogs",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_PosUsers_Last_Modified_By_Id",
                table: "DrawerLogs",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_PosUsers_OpenedById",
                table: "DrawerLogs",
                column: "OpenedById",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_Sites_Site_Id",
                table: "DrawerLogs",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DrawerLogs_Tills_Till_Id",
                table: "DrawerLogs",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorLogs_PosUsers_Resolved_By_Id",
                table: "ErrorLogs",
                column: "Resolved_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorLogs_PosUsers_User_Id",
                table: "ErrorLogs",
                column: "User_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorLogs_Sites_Site_Id",
                table: "ErrorLogs",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorLogs_Tills_Till_Id",
                table: "ErrorLogs",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PosUsers_Sites_Primary_Site_Id",
                table: "PosUsers",
                column: "Primary_Site_Id",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PosUsers_Sites_Site_Id",
                table: "PosUsers",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PosUsers_Tills_Till_Id",
                table: "PosUsers",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PosUsers_Created_By_Id",
                table: "Products",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PosUsers_Last_Modified_By_Id",
                table: "Products",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Promotions_Promotion_Id",
                table: "Products",
                column: "Promotion_Id",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Sites_Site_Id",
                table: "Products",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Tills_Till_Id",
                table: "Products",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesItemTransactions_Payouts_SalesPayout_ID",
                table: "SalesItemTransactions",
                column: "SalesPayout_ID",
                principalTable: "Payouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesItemTransactions_PosUsers_Created_By_Id",
                table: "SalesItemTransactions",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesItemTransactions_PosUsers_Last_Modified_By_Id",
                table: "SalesItemTransactions",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesItemTransactions_Products_Product_ID",
                table: "SalesItemTransactions",
                column: "Product_ID",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesItemTransactions_Promotions_Promotion_ID",
                table: "SalesItemTransactions",
                column: "Promotion_ID",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesTransactions_PosUsers_Created_By_Id",
                table: "SalesTransactions",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesTransactions_PosUsers_Last_Modified_By_Id",
                table: "SalesTransactions",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesTransactions_Sites_Site_Id",
                table: "SalesTransactions",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesTransactions_Tills_Till_Id",
                table: "SalesTransactions",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_PosUsers_PosUser_Id",
                table: "Shifts",
                column: "PosUser_Id",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Sites_Site_Id",
                table: "Shifts",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Tills_Till_Id",
                table: "Shifts",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_PosUsers_Created_By_Id",
                table: "Sites",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_PosUsers_Last_Modified_By_Id",
                table: "Sites",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockRefills_PosUsers_Created_By_ID",
                table: "StockRefills",
                column: "Created_By_ID",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockRefills_PosUsers_Last_Modified_By_ID",
                table: "StockRefills",
                column: "Last_Modified_By_ID",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StockRefills_PosUsers_Refilled_By",
                table: "StockRefills",
                column: "Refilled_By",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_PosUsers_Created_By_Id",
                table: "StockTransactions",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_PosUsers_Last_Modified_By_Id",
                table: "StockTransactions",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Products_ProductId",
                table: "StockTransactions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Sites_From_Site_Id",
                table: "StockTransactions",
                column: "From_Site_Id",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Sites_To_Site_Id",
                table: "StockTransactions",
                column: "To_Site_Id",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Tills_Till_Id",
                table: "StockTransactions",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UnknownProducts_DayLogs_DaylogId",
                table: "UnknownProducts",
                column: "DaylogId",
                principalTable: "DayLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UnknownProducts_PosUsers_CreatedById",
                table: "UnknownProducts",
                column: "CreatedById",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UnknownProducts_Shifts_ShiftId",
                table: "UnknownProducts",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UnknownProducts_Sites_SiteId",
                table: "UnknownProducts",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UnknownProducts_Tills_TillId",
                table: "UnknownProducts",
                column: "TillId",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_PosUsers_Created_By_Id",
                table: "Vouchers",
                column: "Created_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_PosUsers_Last_Modified_By_Id",
                table: "Vouchers",
                column: "Last_Modified_By_Id",
                principalTable: "PosUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_Sites_Site_Id",
                table: "Vouchers",
                column: "Site_Id",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_Tills_Till_Id",
                table: "Vouchers",
                column: "Till_Id",
                principalTable: "Tills",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
