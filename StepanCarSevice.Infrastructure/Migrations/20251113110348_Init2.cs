using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepanCarSevice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InStock",
                table: "Details");

            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "Details",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count",
                table: "Details");

            migrationBuilder.AddColumn<bool>(
                name: "InStock",
                table: "Details",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
