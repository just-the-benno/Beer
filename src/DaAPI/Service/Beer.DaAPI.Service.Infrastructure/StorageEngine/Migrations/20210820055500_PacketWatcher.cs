using Microsoft.EntityFrameworkCore.Migrations;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine.Migrations
{
    public partial class PacketWatcher : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LeasedAddressInResponse",
                table: "DHCPv6PacketEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeasedPrefix",
                table: "DHCPv6PacketEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "LeasedPrefixLength",
                table: "DHCPv6PacketEntries",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "MacAddress",
                table: "DHCPv6PacketEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedAddress",
                table: "DHCPv6PacketEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedPrefix",
                table: "DHCPv6PacketEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "RequestedPrefixLength",
                table: "DHCPv6PacketEntries",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "DHCPv6PacketEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ClientIdentifiier",
                table: "DHCPv4PacketEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeasedAddressInResponse",
                table: "DHCPv4PacketEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MacAddress",
                table: "DHCPv4PacketEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedAddress",
                table: "DHCPv4PacketEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "DHCPv4PacketEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeasedAddressInResponse",
                table: "DHCPv6PacketEntries");

            migrationBuilder.DropColumn(
                name: "LeasedPrefix",
                table: "DHCPv6PacketEntries");

            migrationBuilder.DropColumn(
                name: "LeasedPrefixLength",
                table: "DHCPv6PacketEntries");

            migrationBuilder.DropColumn(
                name: "MacAddress",
                table: "DHCPv6PacketEntries");

            migrationBuilder.DropColumn(
                name: "RequestedAddress",
                table: "DHCPv6PacketEntries");

            migrationBuilder.DropColumn(
                name: "RequestedPrefix",
                table: "DHCPv6PacketEntries");

            migrationBuilder.DropColumn(
                name: "RequestedPrefixLength",
                table: "DHCPv6PacketEntries");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "DHCPv6PacketEntries");

            migrationBuilder.DropColumn(
                name: "ClientIdentifiier",
                table: "DHCPv4PacketEntries");

            migrationBuilder.DropColumn(
                name: "LeasedAddressInResponse",
                table: "DHCPv4PacketEntries");

            migrationBuilder.DropColumn(
                name: "MacAddress",
                table: "DHCPv4PacketEntries");

            migrationBuilder.DropColumn(
                name: "RequestedAddress",
                table: "DHCPv4PacketEntries");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "DHCPv4PacketEntries");
        }
    }
}
