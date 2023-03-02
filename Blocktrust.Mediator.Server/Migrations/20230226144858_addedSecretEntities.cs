using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class addedSecretEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Secrets");
        }
    }
}
