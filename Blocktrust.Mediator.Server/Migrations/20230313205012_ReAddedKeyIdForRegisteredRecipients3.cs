using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class ReAddedKeyIdForRegisteredRecipients3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_StoredMessageEntityId",
                table: "StoredMessages");

            migrationBuilder.DropColumn(
                name: "RecipientDid",
                table: "StoredMessages");

            migrationBuilder.AddColumn<Guid>(
                name: "RegisteredRecipientId",
                table: "StoredMessages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_StoredMessages_RegisteredRecipientId",
                table: "StoredMessages",
                column: "RegisteredRecipientId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_RegisteredRecipientId",
                table: "StoredMessages",
                column: "RegisteredRecipientId",
                principalTable: "RegisteredRecipients",
                principalColumn: "RegisteredRecipientId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_RegisteredRecipientId",
                table: "StoredMessages");

            migrationBuilder.DropIndex(
                name: "IX_StoredMessages_RegisteredRecipientId",
                table: "StoredMessages");

            migrationBuilder.DropColumn(
                name: "RegisteredRecipientId",
                table: "StoredMessages");

            migrationBuilder.AddColumn<string>(
                name: "RecipientDid",
                table: "StoredMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_StoredMessageEntityId",
                table: "StoredMessages",
                column: "StoredMessageEntityId",
                principalTable: "RegisteredRecipients",
                principalColumn: "RegisteredRecipientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
