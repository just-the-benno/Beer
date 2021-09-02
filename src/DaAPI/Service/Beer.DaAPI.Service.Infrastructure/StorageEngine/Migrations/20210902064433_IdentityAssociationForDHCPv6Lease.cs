using Microsoft.EntityFrameworkCore.Migrations;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine.Migrations
{
    public partial class IdentityAssociationForDHCPv6Lease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IdentityAssocationId",
                table: "DHCPv6LeaseEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "IdentityAssocationIdForPrefix",
                table: "DHCPv6LeaseEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityAssocationId",
                table: "DHCPv6LeaseEntries");

            migrationBuilder.DropColumn(
                name: "IdentityAssocationIdForPrefix",
                table: "DHCPv6LeaseEntries");
        }
    }
}
