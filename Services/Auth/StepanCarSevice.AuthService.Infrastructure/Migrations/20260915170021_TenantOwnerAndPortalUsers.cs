using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepanCarSevice.AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TenantOwnerAndPortalUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "Users",
                type: "character varying(64)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)");

            migrationBuilder.AddColumn<int>(
                name: "OwnerUserId",
                table: "TenantInfoEntity",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "TenantInfoEntity");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "Users",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldNullable: true);
        }
    }
}
