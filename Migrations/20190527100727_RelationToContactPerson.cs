using Microsoft.EntityFrameworkCore.Migrations;

namespace Client.Migrations
{
    public partial class RelationToContactPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CustomerId",
                table: "ContactPersons",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "ContactPersons",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CustomerId",
                value: 1L);

            migrationBuilder.CreateIndex(
                name: "IX_ContactPersons_CustomerId",
                table: "ContactPersons",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactPersons_Customers_CustomerId",
                table: "ContactPersons",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactPersons_Customers_CustomerId",
                table: "ContactPersons");

            migrationBuilder.DropIndex(
                name: "IX_ContactPersons_CustomerId",
                table: "ContactPersons");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "ContactPersons");
        }
    }
}
