using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepanCarSevice.DetailService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteTenantData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarManufactures_TenantInfoEntity_TenantId",
                table: "CarManufactures");

            migrationBuilder.DropForeignKey(
                name: "FK_CarModels_CarManufactures_ManufactureId",
                table: "CarModels");

            migrationBuilder.DropForeignKey(
                name: "FK_CarModels_TenantInfoEntity_TenantId",
                table: "CarModels");

            migrationBuilder.DropForeignKey(
                name: "FK_CarModifications_CarModels_CarModelId",
                table: "CarModifications");

            migrationBuilder.DropForeignKey(
                name: "FK_CarModifications_TenantInfoEntity_TenantId",
                table: "CarModifications");

            migrationBuilder.DropForeignKey(
                name: "FK_DetailManufactures_TenantInfoEntity_TenantId",
                table: "DetailManufactures");

            migrationBuilder.DropForeignKey(
                name: "FK_Details_DetailManufactures_DetailManufactureId",
                table: "Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Details_TenantInfoEntity_TenantId",
                table: "Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Engines_TenantInfoEntity_TenantId",
                table: "Engines");

            migrationBuilder.AddForeignKey(
                name: "FK_CarManufactures_TenantInfoEntity_TenantId",
                table: "CarManufactures",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CarModels_CarManufactures_ManufactureId",
                table: "CarModels",
                column: "ManufactureId",
                principalTable: "CarManufactures",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CarModels_TenantInfoEntity_TenantId",
                table: "CarModels",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CarModifications_CarModels_CarModelId",
                table: "CarModifications",
                column: "CarModelId",
                principalTable: "CarModels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CarModifications_TenantInfoEntity_TenantId",
                table: "CarModifications",
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
                name: "FK_Details_DetailManufactures_DetailManufactureId",
                table: "Details",
                column: "DetailManufactureId",
                principalTable: "DetailManufactures",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Details_TenantInfoEntity_TenantId",
                table: "Details",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Engines_TenantInfoEntity_TenantId",
                table: "Engines",
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
                name: "FK_CarModels_CarManufactures_ManufactureId",
                table: "CarModels");

            migrationBuilder.DropForeignKey(
                name: "FK_CarModels_TenantInfoEntity_TenantId",
                table: "CarModels");

            migrationBuilder.DropForeignKey(
                name: "FK_CarModifications_CarModels_CarModelId",
                table: "CarModifications");

            migrationBuilder.DropForeignKey(
                name: "FK_CarModifications_TenantInfoEntity_TenantId",
                table: "CarModifications");

            migrationBuilder.DropForeignKey(
                name: "FK_DetailManufactures_TenantInfoEntity_TenantId",
                table: "DetailManufactures");

            migrationBuilder.DropForeignKey(
                name: "FK_Details_DetailManufactures_DetailManufactureId",
                table: "Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Details_TenantInfoEntity_TenantId",
                table: "Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Engines_TenantInfoEntity_TenantId",
                table: "Engines");

            migrationBuilder.AddForeignKey(
                name: "FK_CarManufactures_TenantInfoEntity_TenantId",
                table: "CarManufactures",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CarModels_CarManufactures_ManufactureId",
                table: "CarModels",
                column: "ManufactureId",
                principalTable: "CarManufactures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CarModels_TenantInfoEntity_TenantId",
                table: "CarModels",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CarModifications_CarModels_CarModelId",
                table: "CarModifications",
                column: "CarModelId",
                principalTable: "CarModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CarModifications_TenantInfoEntity_TenantId",
                table: "CarModifications",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetailManufactures_TenantInfoEntity_TenantId",
                table: "DetailManufactures",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Details_DetailManufactures_DetailManufactureId",
                table: "Details",
                column: "DetailManufactureId",
                principalTable: "DetailManufactures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Details_TenantInfoEntity_TenantId",
                table: "Details",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Engines_TenantInfoEntity_TenantId",
                table: "Engines",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
