using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyNames5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Key",
                table: "RecipientKeys",
                newName: "RecipientKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecipientKey",
                table: "RecipientKeys",
                newName: "Key");
        }
    }
}
