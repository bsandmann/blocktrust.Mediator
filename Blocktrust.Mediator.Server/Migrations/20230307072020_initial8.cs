using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class initial8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_RegisteredRecipientRecipientDid",
                table: "StoredMessages");

            migrationBuilder.RenameColumn(
                name: "RegisteredRecipientRecipientDid",
                table: "StoredMessages",
                newName: "RecipientDid");

            migrationBuilder.RenameIndex(
                name: "IX_StoredMessages_RegisteredRecipientRecipientDid",
                table: "StoredMessages",
                newName: "IX_StoredMessages_RecipientDid");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_RecipientDid",
                table: "StoredMessages",
                column: "RecipientDid",
                principalTable: "RegisteredRecipients",
                principalColumn: "RecipientDid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_RecipientDid",
                table: "StoredMessages");

            migrationBuilder.RenameColumn(
                name: "RecipientDid",
                table: "StoredMessages",
                newName: "RegisteredRecipientRecipientDid");

            migrationBuilder.RenameIndex(
                name: "IX_StoredMessages_RecipientDid",
                table: "StoredMessages",
                newName: "IX_StoredMessages_RegisteredRecipientRecipientDid");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_RegisteredRecipientRecipientDid",
                table: "StoredMessages",
                column: "RegisteredRecipientRecipientDid",
                principalTable: "RegisteredRecipients",
                principalColumn: "RecipientDid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
