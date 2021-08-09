using Microsoft.EntityFrameworkCore.Migrations;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine.Migrations
{
    public partial class TracingStreamsWithErrors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasFailed",
                table: "TracingStreams");

            migrationBuilder.DropColumn(
                name: "IsError",
                table: "TracingStreamEntries");
        }
    }
}
