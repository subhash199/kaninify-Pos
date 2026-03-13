using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class Business_Settings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RefillEnforcement_Enabled = table.Column<int>(type: "integer", nullable: false),
                    AllowShiftEndWithPendingRefill = table.Column<bool>(type: "boolean", nullable: false),
                    AllowDayEndWithPendingRefills = table.Column<bool>(type: "boolean", nullable: false),
                    PrintSalesReceiptForEverySale = table.Column<bool>(type: "boolean", nullable: false),
                    PrintRefundReceiptForEveryRefund = table.Column<bool>(type: "boolean", nullable: false),
                    PrintPayoutReceiptForEveryPayout = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_BusinessSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessSettings_PosUsers_Created_By_Id",
                        column: x => x.Created_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessSettings_PosUsers_Last_Modified_By_Id",
                        column: x => x.Last_Modified_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessSettings_Sites_Site_Id",
                        column: x => x.Site_Id,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessSettings_Tills_Till_Id",
                        column: x => x.Till_Id,
                        principalTable: "Tills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSettings_Created_By_Id",
                table: "BusinessSettings",
                column: "Created_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSettings_Last_Modified_By_Id",
                table: "BusinessSettings",
                column: "Last_Modified_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSettings_Site_Id",
                table: "BusinessSettings",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSettings_Till_Id",
                table: "BusinessSettings",
                column: "Till_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessSettings");
        }
    }
}
