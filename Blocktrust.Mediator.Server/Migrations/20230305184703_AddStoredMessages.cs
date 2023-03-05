using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediatorConnectionKeyEntity_Connections_MediatorConnectionKeyId",
                table: "MediatorConnectionKeyEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MediatorConnectionKeyEntity",
                table: "MediatorConnectionKeyEntity");

            migrationBuilder.RenameTable(
                name: "MediatorConnectionKeyEntity",
                newName: "RecipientKeys");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecipientKeys",
                table: "RecipientKeys",
                column: "MediatorConnectionKeyId");

            migrationBuilder.CreateTable(
                name: "StoredMessages",
                columns: table => new
                {
                    StoredMessageEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MediatorConnectionKeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredMessages", x => x.StoredMessageEntityId);
                    table.ForeignKey(
                        name: "FK_StoredMessages_RecipientKeys_StoredMessageEntityId",
                        column: x => x.StoredMessageEntityId,
                        principalTable: "RecipientKeys",
                        principalColumn: "MediatorConnectionKeyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_RecipientKeys_Connections_MediatorConnectionKeyId",
                table: "RecipientKeys",
                column: "MediatorConnectionKeyId",
                principalTable: "Connections",
                principalColumn: "MediatorConnectionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipientKeys_Connections_MediatorConnectionKeyId",
                table: "RecipientKeys");

            migrationBuilder.DropTable(
                name: "StoredMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecipientKeys",
                table: "RecipientKeys");

            migrationBuilder.RenameTable(
                name: "RecipientKeys",
                newName: "MediatorConnectionKeyEntity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MediatorConnectionKeyEntity",
                table: "MediatorConnectionKeyEntity",
                column: "MediatorConnectionKeyId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediatorConnectionKeyEntity_Connections_MediatorConnectionKeyId",
                table: "MediatorConnectionKeyEntity",
                column: "MediatorConnectionKeyId",
                principalTable: "Connections",
                principalColumn: "MediatorConnectionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
