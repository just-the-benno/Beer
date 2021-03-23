using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DHCPv4Interfaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IPv4Address = table.Column<string>(type: "text", nullable: true),
                    InterfaceId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DHCPv4Interfaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DHCPv4LeaseEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Start = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndReason = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DHCPv4LeaseEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DHCPv4PacketEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestType = table.Column<int>(type: "integer", nullable: false),
                    RequestSize = table.Column<int>(type: "integer", nullable: false),
                    RequestDestination = table.Column<string>(type: "text", nullable: true),
                    RequestSource = table.Column<string>(type: "text", nullable: true),
                    RequestStream = table.Column<byte[]>(type: "bytea", nullable: true),
                    ResponseType = table.Column<int>(type: "integer", nullable: true),
                    ResponseSize = table.Column<int>(type: "integer", nullable: true),
                    ResponseDestination = table.Column<string>(type: "text", nullable: true),
                    ResponseSource = table.Column<string>(type: "text", nullable: true),
                    ResponseStream = table.Column<byte[]>(type: "bytea", nullable: true),
                    HandledSuccessfully = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorCode = table.Column<int>(type: "integer", nullable: false),
                    FilteredBy = table.Column<string>(type: "text", nullable: true),
                    InvalidRequest = table.Column<bool>(type: "boolean", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimestampDay = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimestampWeek = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimestampMonth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DHCPv4PacketEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DHCPv6Interfaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IPv6Address = table.Column<string>(type: "text", nullable: true),
                    InterfaceId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DHCPv6Interfaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DHCPv6LeaseEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Prefix = table.Column<string>(type: "text", nullable: true),
                    PrefixLength = table.Column<byte>(type: "smallint", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndReason = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DHCPv6LeaseEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DHCPv6PacketEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestType = table.Column<byte>(type: "smallint", nullable: false),
                    RequestSize = table.Column<int>(type: "integer", nullable: false),
                    RequestDestination = table.Column<string>(type: "text", nullable: true),
                    RequestSource = table.Column<string>(type: "text", nullable: true),
                    RequestStream = table.Column<byte[]>(type: "bytea", nullable: true),
                    ResponseType = table.Column<byte>(type: "smallint", nullable: true),
                    ResponseSize = table.Column<int>(type: "integer", nullable: true),
                    ResponseDestination = table.Column<string>(type: "text", nullable: true),
                    ResponseSource = table.Column<string>(type: "text", nullable: true),
                    ResponseStream = table.Column<byte[]>(type: "bytea", nullable: true),
                    HandledSuccessfully = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorCode = table.Column<int>(type: "integer", nullable: false),
                    FilteredBy = table.Column<string>(type: "text", nullable: true),
                    InvalidRequest = table.Column<bool>(type: "boolean", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimestampDay = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimestampWeek = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimestampMonth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DHCPv6PacketEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Helper",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Helper", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "NotificationPipelineEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPipelineEntries", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DHCPv4Interfaces");

            migrationBuilder.DropTable(
                name: "DHCPv4LeaseEntries");

            migrationBuilder.DropTable(
                name: "DHCPv4PacketEntries");

            migrationBuilder.DropTable(
                name: "DHCPv6Interfaces");

            migrationBuilder.DropTable(
                name: "DHCPv6LeaseEntries");

            migrationBuilder.DropTable(
                name: "DHCPv6PacketEntries");

            migrationBuilder.DropTable(
                name: "Helper");

            migrationBuilder.DropTable(
                name: "NotificationPipelineEntries");
        }
    }
}
