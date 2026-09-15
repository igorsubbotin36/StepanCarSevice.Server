using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepanCarSevice.AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SecurityStampAndUniquePhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Существующим пользователям нужна непустая метка, иначе их токены не пройдут проверку
            migrationBuilder.Sql("UPDATE \"Users\" SET \"SecurityStamp\" = md5(random()::text || clock_timestamp()::text || \"Id\"::text) WHERE \"SecurityStamp\" = '';");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Phone",
                table: "Users",
                columns: new[] { "TenantId", "Phone" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Phone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");
        }
    }
}
