using Microsoft.EntityFrameworkCore.Migrations;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine.Migrations
{
    public partial class TracingStreamsWithErrors2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasFailed",
                table: "TracingStreams");

            migrationBuilder.DropColumn(
                name: "IsError",
                table: "TracingStreamEntries");

            migrationBuilder.AddColumn<int>(
                name: "ResultType",
                table: "TracingStreams",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ResultType",
                table: "TracingStreamEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResultType",
                table: "TracingStreams");

            migrationBuilder.DropColumn(
                name: "ResultType",
                table: "TracingStreamEntries");

            migrationBuilder.AddColumn<bool>(
                name: "HasFailed",
                table: "TracingStreams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsError",
                table: "TracingStreamEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
