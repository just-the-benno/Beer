using Microsoft.EntityFrameworkCore.Migrations;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine.Migrations
{
    public partial class OrderingNumberForDHCPv4Leases : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OrderNumber",
                table: "DHCPv4LeaseEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "DHCPv4LeaseEntries");
        }
    }
}
