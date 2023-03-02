using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class addedConnectionsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Connections",
                columns: table => new
                {
                    MediatorConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediatorDid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RemoteDid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MediatorEndpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MediationGranted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.MediatorConnectionId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Connections");
        }
    }
}
