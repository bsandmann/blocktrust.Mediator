using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class initial1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Connections",
                columns: table => new
                {
                    MediatorConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediatorDid = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RemoteDid = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoutingDid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediatorEndpoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediationGranted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.MediatorConnectionId);
                });

            migrationBuilder.CreateTable(
                name: "OobInvitations",
                columns: table => new
                {
                    OobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Did = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Invitation = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OobInvitations", x => x.OobId);
                });

            migrationBuilder.CreateTable(
                name: "Secrets",
                columns: table => new
                {
                    SecretId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerificationMethodType = table.Column<int>(type: "int", nullable: false),
                    VerificationMaterialFormat = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Secrets", x => x.SecretId);
                });

            migrationBuilder.CreateTable(
                name: "RecipientKeys",
                columns: table => new
                {
                    RecipientDid = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MediatorConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipientKeys", x => x.RecipientDid);
                    table.ForeignKey(
                        name: "FK_RecipientKeys_Connections_MediatorConnectionId",
                        column: x => x.MediatorConnectionId,
                        principalTable: "Connections",
                        principalColumn: "MediatorConnectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoredMessages",
                columns: table => new
                {
                    StoredMessageEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessageId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageSize = table.Column<long>(type: "bigint", nullable: false),
                    RegisteredRecipientRecipientDid = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredMessages", x => x.StoredMessageEntityId);
                    table.ForeignKey(
                        name: "FK_StoredMessages_RecipientKeys_RegisteredRecipientRecipientDid",
                        column: x => x.RegisteredRecipientRecipientDid,
                        principalTable: "RecipientKeys",
                        principalColumn: "RecipientDid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Connections_RemoteDid_MediatorDid",
                table: "Connections",
                columns: new[] { "RemoteDid", "MediatorDid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipientKeys_MediatorConnectionId",
                table: "RecipientKeys",
                column: "MediatorConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredMessages_RegisteredRecipientRecipientDid",
                table: "StoredMessages",
                column: "RegisteredRecipientRecipientDid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OobInvitations");

            migrationBuilder.DropTable(
                name: "Secrets");

            migrationBuilder.DropTable(
                name: "StoredMessages");

            migrationBuilder.DropTable(
                name: "RecipientKeys");

            migrationBuilder.DropTable(
                name: "Connections");
        }
    }
}
