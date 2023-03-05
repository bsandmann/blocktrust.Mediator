using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyNames4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipientKeys_Connections_MediatorConnectionKeyId",
                table: "RecipientKeys");

            migrationBuilder.RenameColumn(
                name: "MediatorConnectionKeyId",
                table: "RecipientKeys",
                newName: "ConnectionKeyEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipientKeys_Connections_ConnectionKeyEntityId",
                table: "RecipientKeys",
                column: "ConnectionKeyEntityId",
                principalTable: "Connections",
                principalColumn: "ConnectionEntityId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipientKeys_Connections_ConnectionKeyEntityId",
                table: "RecipientKeys");

            migrationBuilder.RenameColumn(
                name: "ConnectionKeyEntityId",
                table: "RecipientKeys",
                newName: "MediatorConnectionKeyId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipientKeys_Connections_MediatorConnectionKeyId",
                table: "RecipientKeys",
                column: "MediatorConnectionKeyId",
                principalTable: "Connections",
                principalColumn: "ConnectionEntityId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
