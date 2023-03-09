using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class addingShortenedUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShortenedUrlEntities",
                columns: table => new
                {
                    ShortenedUrlEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpirationUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestedPartialSlug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GoalCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortenedUrlEntities", x => x.ShortenedUrlEntityId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShortenedUrlEntities");
        }
    }
}
