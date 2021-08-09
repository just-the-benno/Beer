using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine.Migrations
{
    public partial class TracingStreams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TracingStreams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SystemIdentifier = table.Column<int>(type: "integer", nullable: false),
                    ProcedureIdentifier = table.Column<int>(type: "integer", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FirstEntryData = table.Column<string>(type: "jsonb", nullable: true),
                    RecordCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TracingStreams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TracingStreamEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    AddtionalData = table.Column<string>(type: "jsonb", nullable: true),
                    StreamId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TracingStreamEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TracingStreamEntries_TracingStreams_StreamId",
                        column: x => x.StreamId,
                        principalTable: "TracingStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TracingStreamEntries_StreamId",
                table: "TracingStreamEntries",
                column: "StreamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TracingStreamEntries");

            migrationBuilder.DropTable(
                name: "TracingStreams");
        }
    }
}
