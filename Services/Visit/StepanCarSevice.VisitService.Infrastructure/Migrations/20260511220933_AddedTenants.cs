using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepanCarSevice.VisitService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedTenants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantInfoEntity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConnectionString = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ApiKey = table.Column<string>(type: "text", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantInfoEntity", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantInfoEntity_Identifier",
                table: "TenantInfoEntity",
                column: "Identifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantInfoEntity");
        }
    }
}
