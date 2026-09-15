using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepanCarService.TenantService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedTenantOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerUserId",
                table: "TenantInfoEntity",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantInfoEntity_OwnerUserId",
                table: "TenantInfoEntity",
                column: "OwnerUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantInfoEntity_OwnerUserId",
                table: "TenantInfoEntity");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "TenantInfoEntity");
        }
    }
}
