using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepanCarSevice.DetailService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditedEntitiesAndAddedInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EngineTypes_TenantInfoEntity_TenantId",
                table: "EngineTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_TransmissionTypes_TenantInfoEntity_TenantId",
                table: "TransmissionTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_WheelDriveTypes_TenantInfoEntity_TenantId",
                table: "WheelDriveTypes");

            migrationBuilder.DropIndex(
                name: "IX_WheelDriveTypes_TenantId",
                table: "WheelDriveTypes");

            migrationBuilder.DropIndex(
                name: "IX_TransmissionTypes_TenantId",
                table: "TransmissionTypes");

            migrationBuilder.DropIndex(
                name: "IX_EngineTypes_TenantId",
                table: "EngineTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "WheelDriveTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TransmissionTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EngineTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "WheelDriveTypes",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "TransmissionTypes",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "EngineTypes",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_WheelDriveTypes_TenantId",
                table: "WheelDriveTypes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TransmissionTypes_TenantId",
                table: "TransmissionTypes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineTypes_TenantId",
                table: "EngineTypes",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_EngineTypes_TenantInfoEntity_TenantId",
                table: "EngineTypes",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransmissionTypes_TenantInfoEntity_TenantId",
                table: "TransmissionTypes",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WheelDriveTypes_TenantInfoEntity_TenantId",
                table: "WheelDriveTypes",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
