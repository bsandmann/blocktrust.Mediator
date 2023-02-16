using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class renamingEntityInDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Oobs",
                table: "Oobs");

            migrationBuilder.RenameTable(
                name: "Oobs",
                newName: "OobInvitations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OobInvitations",
                table: "OobInvitations",
                column: "OobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OobInvitations",
                table: "OobInvitations");

            migrationBuilder.RenameTable(
                name: "OobInvitations",
                newName: "Oobs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Oobs",
                table: "Oobs",
                column: "OobId");
        }
    }
}
