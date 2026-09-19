using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTerminalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentTerminalSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Display_Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Merchant_Display_Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Store_Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Store_Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Store_City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Store_Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Terminal_Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Terminal_Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Terminal_Serial_Number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Currency_Code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Epos_Instance_Id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Is_Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Is_Pay_At_Counter_Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Access_Token = table.Column<string>(type: "text", nullable: true),
                    Refresh_Token = table.Column<string>(type: "text", nullable: true),
                    Access_Token_Expires_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Last_Verified_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Last_Known_Status = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Last_Modified_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTerminalSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTerminalSettings_PosUsers_Created_By_Id",
                        column: x => x.Created_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTerminalSettings_PosUsers_Last_Modified_By_Id",
                        column: x => x.Last_Modified_By_Id,
                        principalTable: "PosUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTerminalSettings_Sites_Site_Id",
                        column: x => x.Site_Id,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTerminalSettings_Tills_Till_Id",
                        column: x => x.Till_Id,
                        principalTable: "Tills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerminalSetting_Provider_Site_Till",
                table: "PaymentTerminalSettings",
                columns: new[] { "Provider", "Site_Id", "Till_Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerminalSetting_SyncStatus",
                table: "PaymentTerminalSettings",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerminalSettings_Created_By_Id",
                table: "PaymentTerminalSettings",
                column: "Created_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerminalSettings_Last_Modified_By_Id",
                table: "PaymentTerminalSettings",
                column: "Last_Modified_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerminalSettings_Site_Id",
                table: "PaymentTerminalSettings",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerminalSettings_Till_Id",
                table: "PaymentTerminalSettings",
                column: "Till_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentTerminalSettings");
        }
    }
}
