using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepanCarSevice.DetailService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Details",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "DetailManufactures",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "CarModels",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "CarManufactures",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Details_TenantId",
                table: "Details",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailManufactures_TenantId",
                table: "DetailManufactures",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CarModels_TenantId",
                table: "CarModels",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CarManufactures_TenantId",
                table: "CarManufactures",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarManufactures_TenantInfoEntity_TenantId",
                table: "CarManufactures",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CarModels_TenantInfoEntity_TenantId",
                table: "CarModels",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetailManufactures_TenantInfoEntity_TenantId",
                table: "DetailManufactures",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Details_TenantInfoEntity_TenantId",
                table: "Details",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarManufactures_TenantInfoEntity_TenantId",
                table: "CarManufactures");

            migrationBuilder.DropForeignKey(
                name: "FK_CarModels_TenantInfoEntity_TenantId",
                table: "CarModels");

            migrationBuilder.DropForeignKey(
                name: "FK_DetailManufactures_TenantInfoEntity_TenantId",
                table: "DetailManufactures");

            migrationBuilder.DropForeignKey(
                name: "FK_Details_TenantInfoEntity_TenantId",
                table: "Details");

            migrationBuilder.DropIndex(
                name: "IX_Details_TenantId",
                table: "Details");

            migrationBuilder.DropIndex(
                name: "IX_DetailManufactures_TenantId",
                table: "DetailManufactures");

            migrationBuilder.DropIndex(
                name: "IX_CarModels_TenantId",
                table: "CarModels");

            migrationBuilder.DropIndex(
                name: "IX_CarManufactures_TenantId",
                table: "CarManufactures");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Details");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DetailManufactures");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CarModels");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CarManufactures");
        }
    }
}
