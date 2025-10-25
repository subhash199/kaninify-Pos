using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class deliveryandvoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InvoiceTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    InvoiceVatTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalItemsDelivered = table.Column<int>(type: "integer", nullable: false),
                    SyncStatus = table.Column<int>(type: "integer", nullable: false),
                    DayLogId = table.Column<int>(type: "integer", nullable: true),
                    Shift_Id = table.Column<int>(type: "integer", nullable: true),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryInvoices_DayLogs_DayLogId",
                        column: x => x.DayLogId,
                        principalTable: "DayLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeliveryInvoices_PosUsers_Created_By_Id",
                        column: x => x.Created_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeliveryInvoices_PosUsers_Last_Modified_By_Id",
                        column: x => x.Last_Modified_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeliveryInvoices_Shifts_Shift_Id",
                        column: x => x.Shift_Id,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeliveryInvoices_Sites_Site_Id",
                        column: x => x.Site_Id,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeliveryInvoices_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeliveryInvoices_Tills_Till_Id",
                        column: x => x.Till_Id,
                        principalTable: "Tills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Voucher_Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Voucher_Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Voucher_Value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Is_Percentage = table.Column<bool>(type: "boolean", nullable: false),
                    Min_Basket_Total = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Valid_From = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Valid_To = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Is_Active = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vouchers_PosUsers_Created_By_Id",
                        column: x => x.Created_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Vouchers_PosUsers_Last_Modified_By_Id",
                        column: x => x.Last_Modified_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Vouchers_Sites_Site_Id",
                        column: x => x.Site_Id,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Vouchers_Tills_Till_Id",
                        column: x => x.Till_Id,
                        principalTable: "Tills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeliveryInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    QuantityDelivered = table.Column<int>(type: "integer", nullable: false),
                    CostPerCase = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SupplierItemId = table.Column<int>(type: "integer", nullable: true),
                    ConfirmedByScanner = table.Column<bool>(type: "boolean", nullable: false),
                    SyncStatus = table.Column<int>(type: "integer", nullable: false),
                    DeliveryInvoiceId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryItems_DeliveryInvoices_DeliveryInvoiceId",
                        column: x => x.DeliveryInvoiceId,
                        principalTable: "DeliveryInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryItems_DeliveryInvoices_DeliveryInvoiceId1",
                        column: x => x.DeliveryInvoiceId1,
                        principalTable: "DeliveryInvoices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryItems_SupplierItems_SupplierItemId",
                        column: x => x.SupplierItemId,
                        principalTable: "SupplierItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoucherDepartmentExclusions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VoucherId = table.Column<int>(type: "integer", nullable: false),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    VoucherId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherDepartmentExclusions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherDepartmentExclusions_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoucherDepartmentExclusions_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoucherDepartmentExclusions_Vouchers_VoucherId1",
                        column: x => x.VoucherId1,
                        principalTable: "Vouchers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VoucherProductExclusions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VoucherId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    VoucherId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherProductExclusions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherProductExclusions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoucherProductExclusions_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoucherProductExclusions_Vouchers_VoucherId1",
                        column: x => x.VoucherId1,
                        principalTable: "Vouchers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnSyncedLogs_SyncStatus",
                table: "UnSyncedLogs",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_Created_By_Id",
                table: "DeliveryInvoices",
                column: "Created_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_DayLogId",
                table: "DeliveryInvoices",
                column: "DayLogId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_Last_Modified_By_Id",
                table: "DeliveryInvoices",
                column: "Last_Modified_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_Shift_Id",
                table: "DeliveryInvoices",
                column: "Shift_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_Site_Id",
                table: "DeliveryInvoices",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_SupplierId_InvoiceId",
                table: "DeliveryInvoices",
                columns: new[] { "SupplierId", "InvoiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInvoices_Till_Id",
                table: "DeliveryInvoices",
                column: "Till_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryItems_DeliveryInvoiceId",
                table: "DeliveryItems",
                column: "DeliveryInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryItems_DeliveryInvoiceId1",
                table: "DeliveryItems",
                column: "DeliveryInvoiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryItems_ProductId",
                table: "DeliveryItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryItems_SupplierItemId",
                table: "DeliveryItems",
                column: "SupplierItemId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherDepartmentExclusions_DepartmentId",
                table: "VoucherDepartmentExclusions",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherDepartmentExclusions_VoucherId",
                table: "VoucherDepartmentExclusions",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherDepartmentExclusions_VoucherId1",
                table: "VoucherDepartmentExclusions",
                column: "VoucherId1");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherProductExclusions_ProductId",
                table: "VoucherProductExclusions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherProductExclusions_VoucherId",
                table: "VoucherProductExclusions",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherProductExclusions_VoucherId1",
                table: "VoucherProductExclusions",
                column: "VoucherId1");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Created_By_Id",
                table: "Vouchers",
                column: "Created_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Last_Modified_By_Id",
                table: "Vouchers",
                column: "Last_Modified_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Site_Id",
                table: "Vouchers",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Till_Id",
                table: "Vouchers",
                column: "Till_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryItems");

            migrationBuilder.DropTable(
                name: "VoucherDepartmentExclusions");

            migrationBuilder.DropTable(
                name: "VoucherProductExclusions");

            migrationBuilder.DropTable(
                name: "DeliveryInvoices");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_UnSyncedLogs_SyncStatus",
                table: "UnSyncedLogs");
        }
    }
}
