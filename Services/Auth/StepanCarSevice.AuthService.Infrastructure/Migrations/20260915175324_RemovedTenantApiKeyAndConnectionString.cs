using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepanCarSevice.AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedTenantApiKeyAndConnectionString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "TenantInfoEntity");

            migrationBuilder.DropColumn(
                name: "ConnectionString",
                table: "TenantInfoEntity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "TenantInfoEntity",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConnectionString",
                table: "TenantInfoEntity",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
