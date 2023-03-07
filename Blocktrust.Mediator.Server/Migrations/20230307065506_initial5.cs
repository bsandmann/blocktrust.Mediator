using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class initial5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipientKeys_Connections_MediatorConnectionId",
                table: "RecipientKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_StoredMessages_RecipientKeys_RegisteredRecipientRecipientDid",
                table: "StoredMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecipientKeys",
                table: "RecipientKeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Connections",
                table: "Connections");

            migrationBuilder.RenameTable(
                name: "RecipientKeys",
                newName: "RegisteredRecipients");

            migrationBuilder.RenameTable(
                name: "Connections",
                newName: "MediatorConnections");

            migrationBuilder.RenameIndex(
                name: "IX_RecipientKeys_MediatorConnectionId",
                table: "RegisteredRecipients",
                newName: "IX_RegisteredRecipients_MediatorConnectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Connections_RemoteDid_MediatorDid",
                table: "MediatorConnections",
                newName: "IX_MediatorConnections_RemoteDid_MediatorDid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegisteredRecipients",
                table: "RegisteredRecipients",
                column: "RecipientDid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MediatorConnections",
                table: "MediatorConnections",
                column: "MediatorConnectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegisteredRecipients_MediatorConnections_MediatorConnectionId",
                table: "RegisteredRecipients",
                column: "MediatorConnectionId",
                principalTable: "MediatorConnections",
                principalColumn: "MediatorConnectionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_RegisteredRecipientRecipientDid",
                table: "StoredMessages",
                column: "RegisteredRecipientRecipientDid",
                principalTable: "RegisteredRecipients",
                principalColumn: "RecipientDid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegisteredRecipients_MediatorConnections_MediatorConnectionId",
                table: "RegisteredRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_RegisteredRecipientRecipientDid",
                table: "StoredMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegisteredRecipients",
                table: "RegisteredRecipients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MediatorConnections",
                table: "MediatorConnections");

            migrationBuilder.RenameTable(
                name: "RegisteredRecipients",
                newName: "RecipientKeys");

            migrationBuilder.RenameTable(
                name: "MediatorConnections",
                newName: "Connections");

            migrationBuilder.RenameIndex(
                name: "IX_RegisteredRecipients_MediatorConnectionId",
                table: "RecipientKeys",
                newName: "IX_RecipientKeys_MediatorConnectionId");

            migrationBuilder.RenameIndex(
                name: "IX_MediatorConnections_RemoteDid_MediatorDid",
                table: "Connections",
                newName: "IX_Connections_RemoteDid_MediatorDid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecipientKeys",
                table: "RecipientKeys",
                column: "RecipientDid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Connections",
                table: "Connections",
                column: "MediatorConnectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipientKeys_Connections_MediatorConnectionId",
                table: "RecipientKeys",
                column: "MediatorConnectionId",
                principalTable: "Connections",
                principalColumn: "MediatorConnectionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoredMessages_RecipientKeys_RegisteredRecipientRecipientDid",
                table: "StoredMessages",
                column: "RegisteredRecipientRecipientDid",
                principalTable: "RecipientKeys",
                principalColumn: "RecipientDid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
