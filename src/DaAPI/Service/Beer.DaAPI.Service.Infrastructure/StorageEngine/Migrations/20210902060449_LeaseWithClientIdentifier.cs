using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine.Migrations
{
    public partial class LeaseWithClientIdentifier : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ClientIdentifier",
                table: "DHCPv6LeaseEntries",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "UniqueIdentifier",
                table: "DHCPv6LeaseEntries",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ClientIdentifier",
                table: "DHCPv4LeaseEntries",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ClientMacAddress",
                table: "DHCPv4LeaseEntries",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "UniqueIdentifier",
                table: "DHCPv4LeaseEntries",
                type: "bytea",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientIdentifier",
                table: "DHCPv6LeaseEntries");

            migrationBuilder.DropColumn(
                name: "UniqueIdentifier",
                table: "DHCPv6LeaseEntries");

            migrationBuilder.DropColumn(
                name: "ClientIdentifier",
                table: "DHCPv4LeaseEntries");

            migrationBuilder.DropColumn(
                name: "ClientMacAddress",
                table: "DHCPv4LeaseEntries");

            migrationBuilder.DropColumn(
                name: "UniqueIdentifier",
                table: "DHCPv4LeaseEntries");
        }
    }
}
