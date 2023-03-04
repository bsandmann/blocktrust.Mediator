using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class addKeylist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediatorConnectionKeyEntity",
                columns: table => new
                {
                    MediatorConnectionKeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediatorConnectionKeyEntity", x => x.MediatorConnectionKeyId);
                    table.ForeignKey(
                        name: "FK_MediatorConnectionKeyEntity_Connections_MediatorConnectionKeyId",
                        column: x => x.MediatorConnectionKeyId,
                        principalTable: "Connections",
                        principalColumn: "MediatorConnectionId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediatorConnectionKeyEntity");
        }
    }
}
