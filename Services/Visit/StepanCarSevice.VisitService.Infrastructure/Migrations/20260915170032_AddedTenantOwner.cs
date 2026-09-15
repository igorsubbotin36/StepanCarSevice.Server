using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepanCarSevice.VisitService.Infrastructure.Migrations
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "TenantInfoEntity");
        }
    }
}
