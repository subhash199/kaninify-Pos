using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataHandlerLibrary.Migrations
{
    /// <inheritdoc />
    public partial class ShiftCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""Shifts"" DROP CONSTRAINT IF EXISTS ""FK_Shifts_PosUsers_Created_ById"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Shifts"" DROP CONSTRAINT IF EXISTS ""FK_Shifts_PosUsers_Last_Modified_ById"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Shifts_Created_ById"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Shifts_Last_Modified_ById"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Shifts"" DROP COLUMN IF EXISTS ""Created_ById"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Shifts"" DROP COLUMN IF EXISTS ""Last_Modified_ById"";");

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Shifts_Created_By_Id"" ON ""Shifts"" (""Created_By_Id"");");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Shifts_Last_Modified_By_Id"" ON ""Shifts"" (""Last_Modified_By_Id"");");

            migrationBuilder.Sql(@"ALTER TABLE ""Shifts"" DROP CONSTRAINT IF EXISTS ""FK_Shifts_PosUsers_Created_By_Id"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Shifts"" ADD CONSTRAINT ""FK_Shifts_PosUsers_Created_By_Id"" FOREIGN KEY (""Created_By_Id"") REFERENCES ""PosUsers"" (""Id"") ON DELETE SET NULL;");

            migrationBuilder.Sql(@"ALTER TABLE ""Shifts"" DROP CONSTRAINT IF EXISTS ""FK_Shifts_PosUsers_Last_Modified_By_Id"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Shifts"" ADD CONSTRAINT ""FK_Shifts_PosUsers_Last_Modified_By_Id"" FOREIGN KEY (""Last_Modified_By_Id"") REFERENCES ""PosUsers"" (""Id"") ON DELETE SET NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_PosUsers_Last_Modified_By_Id",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_Created_By_Id",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_Last_Modified_By_Id",
                table: "Shifts");

            migrationBuilder.AddColumn<int>(
                name: "Created_ById",
                table: "Shifts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Last_Modified_ById",
                table: "Shifts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_Created_ById",
                table: "Shifts",
                column: "Created_ById");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_Last_Modified_ById",
                table: "Shifts",
                column: "Last_Modified_ById");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_PosUsers_Created_ById",
                table: "Shifts",
                column: "Created_ById",
                principalTable: "PosUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_PosUsers_Last_Modified_ById",
                table: "Shifts",
                column: "Last_Modified_ById",
                principalTable: "PosUsers",
                principalColumn: "Id");
        }
    }
}
