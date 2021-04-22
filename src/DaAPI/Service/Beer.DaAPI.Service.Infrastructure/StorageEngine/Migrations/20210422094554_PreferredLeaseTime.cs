using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine.Migrations
{
    public partial class PreferredLeaseTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndOfPreferredLifetime",
                table: "DHCPv6LeaseEntries",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndOfRenewalTime",
                table: "DHCPv6LeaseEntries",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DHCPv6LeaseEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndOfPreferredLifetime",
                table: "DHCPv4LeaseEntries",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndOfRenewalTime",
                table: "DHCPv4LeaseEntries",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DHCPv4LeaseEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndOfPreferredLifetime",
                table: "DHCPv6LeaseEntries");

            migrationBuilder.DropColumn(
                name: "EndOfRenewalTime",
                table: "DHCPv6LeaseEntries");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DHCPv6LeaseEntries");

            migrationBuilder.DropColumn(
                name: "EndOfPreferredLifetime",
                table: "DHCPv4LeaseEntries");

            migrationBuilder.DropColumn(
                name: "EndOfRenewalTime",
                table: "DHCPv4LeaseEntries");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DHCPv4LeaseEntries");
        }
    }
}
