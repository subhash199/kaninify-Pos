using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddCardTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesTransaction_Id = table.Column<int>(type: "integer", nullable: true),
                    Transaction_Reference = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Environment = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Idempotency_Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Payment_Request_Id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Gateway_Payment_Id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency_Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Transaction_Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status_Reason = table.Column<string>(type: "text", nullable: true),
                    Progress_Status = table.Column<string>(type: "text", nullable: true),
                    Store_Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Terminal_Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Epos_Instance_Id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Merchant_Reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Transaction_Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Date_Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Last_Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By_Id = table.Column<int>(type: "integer", nullable: true),
                    Site_Id = table.Column<int>(type: "integer", nullable: true),
                    Till_Id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardTransactions_SalesTransactions_SalesTransaction_Id",
                        column: x => x.SalesTransaction_Id,
                        principalTable: "SalesTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardTransactions_Gateway_Payment_Id",
                table: "CardTransactions",
                column: "Gateway_Payment_Id");

            migrationBuilder.CreateIndex(
                name: "IX_CardTransactions_Idempotency_Key",
                table: "CardTransactions",
                column: "Idempotency_Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardTransactions_Provider_Environment_Payment_Request_Id",
                table: "CardTransactions",
                columns: new[] { "Provider", "Environment", "Payment_Request_Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardTransactions_SalesTransaction_Id",
                table: "CardTransactions",
                column: "SalesTransaction_Id");

            migrationBuilder.CreateIndex(
                name: "IX_CardTransactions_Transaction_Reference",
                table: "CardTransactions",
                column: "Transaction_Reference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardTransactions");
        }
    }
}
