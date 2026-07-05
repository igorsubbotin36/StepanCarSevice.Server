using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepanCarSevice.AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditedUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_TenantInfoEntity_TenantId",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "Users",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_TenantInfoEntity_TenantId",
                table: "Users",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_TenantInfoEntity_TenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "Users",
                type: "character varying(64)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_TenantInfoEntity_TenantId",
                table: "Users",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id");
        }
    }
}
