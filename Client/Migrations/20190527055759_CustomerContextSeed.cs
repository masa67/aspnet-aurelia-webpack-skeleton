using Microsoft.EntityFrameworkCore.Migrations;

namespace Client.Migrations
{
    public partial class CustomerContextSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ContactPersons",
                columns: new[] { "ContactPersonId", "Name" },
                values: new object[] { 1L, "John Doe" });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "Name", "NameExtension" },
                values: new object[] { 1L, "Company1", "Company1 extension" });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "Name", "NameExtension" },
                values: new object[] { 2L, "Company2", "Company2 extension" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContactPersons",
                keyColumn: "ContactPersonId",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2L);
        }
    }
}
