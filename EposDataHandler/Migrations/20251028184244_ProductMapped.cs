using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class ProductMapped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_PosUsers_Created_ById",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_PosUsers_Last_Modified_ById",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Sites_SiteId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Tills_TillId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Created_ById",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Last_Modified_ById",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SiteId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TillId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Created_ById",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Last_Modified_ById",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TillId",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Created_By_Id",
                table: "Products",
                column: "Created_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Last_Modified_By_Id",
                table: "Products",
                column: "Last_Modified_By_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Site_Id",
                table: "Products",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Till_Id",
                table: "Products",
                column: "Till_Id");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_PosUsers_Created_By_Id",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_PosUsers_Last_Modified_By_Id",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Sites_Site_Id",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Tills_Till_Id",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Created_By_Id",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Last_Modified_By_Id",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Site_Id",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Till_Id",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "Created_ById",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Last_Modified_ById",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SiteId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TillId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Created_ById",
                table: "Products",
                column: "Created_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Last_Modified_ById",
                table: "Products",
                column: "Last_Modified_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SiteId",
                table: "Products",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TillId",
                table: "Products",
                column: "TillId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PosUsers_Created_ById",
                table: "Products",
                column: "Created_ById",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PosUsers_Last_Modified_ById",
                table: "Products",
                column: "Last_Modified_ById",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Sites_SiteId",
                table: "Products",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Tills_TillId",
                table: "Products",
                column: "TillId",
                principalTable: "Tills",
                principalColumn: "Id");
        }
    }
}
