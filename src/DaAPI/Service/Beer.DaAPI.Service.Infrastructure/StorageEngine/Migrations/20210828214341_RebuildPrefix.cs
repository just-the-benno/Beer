using Microsoft.EntityFrameworkCore.Migrations;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine.Migrations
{
    public partial class RebuildPrefix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LeaseddPrefixCombined",
                table: "DHCPv6PacketEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedPrefixCombined",
                table: "DHCPv6PacketEntries",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeaseddPrefixCombined",
                table: "DHCPv6PacketEntries");

            migrationBuilder.DropColumn(
                name: "RequestedPrefixCombined",
                table: "DHCPv6PacketEntries");
        }
    }
}
