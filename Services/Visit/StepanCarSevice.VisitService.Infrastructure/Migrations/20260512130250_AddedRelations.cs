using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepanCarSevice.VisitService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Works",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Visits",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "VisitDetails",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "OwnerSnapshots",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "ManufactureSnapshots",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "DetailSnapshots",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "CarSnapshots",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "CarModelSnapshots",
                type: "character varying(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Works_TenantId",
                table: "Works",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_TenantId",
                table: "Visits",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitDetails_TenantId",
                table: "VisitDetails",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerSnapshots_TenantId",
                table: "OwnerSnapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufactureSnapshots_TenantId",
                table: "ManufactureSnapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailSnapshots_TenantId",
                table: "DetailSnapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSnapshots_TenantId",
                table: "CarSnapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CarModelSnapshots_TenantId",
                table: "CarModelSnapshots",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarModelSnapshots_TenantInfoEntity_TenantId",
                table: "CarModelSnapshots",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CarSnapshots_TenantInfoEntity_TenantId",
                table: "CarSnapshots",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetailSnapshots_TenantInfoEntity_TenantId",
                table: "DetailSnapshots",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManufactureSnapshots_TenantInfoEntity_TenantId",
                table: "ManufactureSnapshots",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OwnerSnapshots_TenantInfoEntity_TenantId",
                table: "OwnerSnapshots",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitDetails_TenantInfoEntity_TenantId",
                table: "VisitDetails",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_TenantInfoEntity_TenantId",
                table: "Visits",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Works_TenantInfoEntity_TenantId",
                table: "Works",
                column: "TenantId",
                principalTable: "TenantInfoEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarModelSnapshots_TenantInfoEntity_TenantId",
                table: "CarModelSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_CarSnapshots_TenantInfoEntity_TenantId",
                table: "CarSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_DetailSnapshots_TenantInfoEntity_TenantId",
                table: "DetailSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_ManufactureSnapshots_TenantInfoEntity_TenantId",
                table: "ManufactureSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_OwnerSnapshots_TenantInfoEntity_TenantId",
                table: "OwnerSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitDetails_TenantInfoEntity_TenantId",
                table: "VisitDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_TenantInfoEntity_TenantId",
                table: "Visits");

            migrationBuilder.DropForeignKey(
                name: "FK_Works_TenantInfoEntity_TenantId",
                table: "Works");

            migrationBuilder.DropIndex(
                name: "IX_Works_TenantId",
                table: "Works");

            migrationBuilder.DropIndex(
                name: "IX_Visits_TenantId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_VisitDetails_TenantId",
                table: "VisitDetails");

            migrationBuilder.DropIndex(
                name: "IX_OwnerSnapshots_TenantId",
                table: "OwnerSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_ManufactureSnapshots_TenantId",
                table: "ManufactureSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_DetailSnapshots_TenantId",
                table: "DetailSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_CarSnapshots_TenantId",
                table: "CarSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_CarModelSnapshots_TenantId",
                table: "CarModelSnapshots");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Works");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "VisitDetails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "OwnerSnapshots");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ManufactureSnapshots");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DetailSnapshots");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CarSnapshots");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CarModelSnapshots");
        }
    }
}
