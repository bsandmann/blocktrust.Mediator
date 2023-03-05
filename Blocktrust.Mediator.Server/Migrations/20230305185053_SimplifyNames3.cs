using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyNames3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MediatorConnectionId",
                table: "Connections",
                newName: "ConnectionEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConnectionEntityId",
                table: "Connections",
                newName: "MediatorConnectionId");
        }
    }
}
