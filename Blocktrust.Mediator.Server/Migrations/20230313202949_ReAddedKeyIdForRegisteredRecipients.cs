using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class ReAddedKeyIdForRegisteredRecipients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_RecipientDid",
                table: "StoredMessages");

            migrationBuilder.DropIndex(
                name: "IX_StoredMessages_RecipientDid",
                table: "StoredMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegisteredRecipients",
                table: "RegisteredRecipients");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientDid",
                table: "StoredMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientDid",
                table: "RegisteredRecipients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "RegisteredRecipientId",
                table: "RegisteredRecipients",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegisteredRecipients",
                table: "RegisteredRecipients",
                column: "RegisteredRecipientId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_StoredMessageEntityId",
                table: "StoredMessages",
                column: "StoredMessageEntityId",
                principalTable: "RegisteredRecipients",
                principalColumn: "RegisteredRecipientId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_StoredMessageEntityId",
                table: "StoredMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegisteredRecipients",
                table: "RegisteredRecipients");

            migrationBuilder.DropColumn(
                name: "RegisteredRecipientId",
                table: "RegisteredRecipients");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientDid",
                table: "StoredMessages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientDid",
                table: "RegisteredRecipients",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegisteredRecipients",
                table: "RegisteredRecipients",
                column: "RecipientDid");

            migrationBuilder.CreateIndex(
                name: "IX_StoredMessages_RecipientDid",
                table: "StoredMessages",
                column: "RecipientDid");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredMessages_RegisteredRecipients_RecipientDid",
                table: "StoredMessages",
                column: "RecipientDid",
                principalTable: "RegisteredRecipients",
                principalColumn: "RecipientDid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
