using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StepanCarSevice.DetailService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedEntities : Migration
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
                name: "FK_DetailManufactures_TenantInfoEntity_TenantId",
                table: "DetailManufactures");

            migrationBuilder.DropForeignKey(
                name: "FK_Details_CarModels_CarModelId",
                table: "Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Details_DetailManufactures_DetailManufactureId",
                table: "Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Details_TenantInfoEntity_TenantId",
                table: "Details");

            migrationBuilder.DropIndex(
                name: "IX_Details_CarModelId",
                table: "Details");

            migrationBuilder.DropColumn(
                name: "CarModelId",
                table: "Details");

            migrationBuilder.DropColumn(
                name: "NameEN",
                table: "CarModels");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "CarManufactures");

            migrationBuilder.DropColumn(
                name: "NameEN",
                table: "CarManufactures");

            migrationBuilder.RenameColumn(
                name: "NameRU",
                table: "CarModels",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameRU",
                table: "CarManufactures",
                newName: "Name");

            migrationBuilder.CreateTable(
                name: "DetailAlternativeRelations",
                columns: table => new
                {
                    DetailId = table.Column<int>(type: "integer", nullable: false),
                    AlternativeDetailId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailAlternativeRelations", x => new { x.DetailId, x.AlternativeDetailId });
                    table.ForeignKey(
                        name: "FK_DetailAlternativeRelations_Details_AlternativeDetailId",
                        column: x => x.AlternativeDetailId,
                        principalTable: "Details",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetailAlternativeRelations_Details_DetailId",
                        column: x => x.DetailId,
                        principalTable: "Details",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetailOriginalRelations",
                columns: table => new
                {
                    DetailId = table.Column<int>(type: "integer", nullable: false),
                    OriginalDetailId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailOriginalRelations", x => new { x.DetailId, x.OriginalDetailId });
                    table.ForeignKey(
                        name: "FK_DetailOriginalRelations_Details_DetailId",
                        column: x => x.DetailId,
                        principalTable: "Details",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetailOriginalRelations_Details_OriginalDetailId",
                        column: x => x.OriginalDetailId,
                        principalTable: "Details",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EngineTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineTypes_TenantInfoEntity_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfoEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransmissionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransmissionTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransmissionTypes_TenantInfoEntity_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfoEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WheelDriveTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WheelDriveTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WheelDriveTypes_TenantInfoEntity_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfoEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Engines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EngineValue = table.Column<string>(type: "text", nullable: true),
                    EngineTypeId = table.Column<int>(type: "integer", nullable: false),
                    CarModelId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Engines_CarModels_CarModelId",
                        column: x => x.CarModelId,
                        principalTable: "CarModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Engines_EngineTypes_EngineTypeId",
                        column: x => x.EngineTypeId,
                        principalTable: "EngineTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Engines_TenantInfoEntity_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfoEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarModifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarModelId = table.Column<int>(type: "integer", nullable: false),
                    EnginePower = table.Column<string>(type: "text", nullable: true),
                    WheelDriveTypeId = table.Column<int>(type: "integer", nullable: false),
                    TransmissionTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarModifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarModifications_CarModels_CarModelId",
                        column: x => x.CarModelId,
                        principalTable: "CarModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarModifications_TenantInfoEntity_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfoEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarModifications_TransmissionTypes_TransmissionTypeId",
                        column: x => x.TransmissionTypeId,
                        principalTable: "TransmissionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarModifications_WheelDriveTypes_WheelDriveTypeId",
                        column: x => x.WheelDriveTypeId,
                        principalTable: "WheelDriveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarModificationDetails",
                columns: table => new
                {
                    CarModificationId = table.Column<int>(type: "integer", nullable: false),
                    DetailId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarModificationDetails", x => new { x.CarModificationId, x.DetailId });
                    table.ForeignKey(
                        name: "FK_CarModificationDetails_CarModifications_CarModificationId",
                        column: x => x.CarModificationId,
                        principalTable: "CarModifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarModificationDetails_Details_DetailId",
                        column: x => x.DetailId,
                        principalTable: "Details",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarModificationEngines",
                columns: table => new
                {
                    CarModificationId = table.Column<int>(type: "integer", nullable: false),
                    EngineId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarModificationEngines", x => new { x.CarModificationId, x.EngineId });
                    table.ForeignKey(
                        name: "FK_CarModificationEngines_CarModifications_CarModificationId",
                        column: x => x.CarModificationId,
                        principalTable: "CarModifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarModificationEngines_Engines_EngineId",
                        column: x => x.EngineId,
                        principalTable: "Engines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarModificationDetails_DetailId",
                table: "CarModificationDetails",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_CarModificationEngines_EngineId",
                table: "CarModificationEngines",
                column: "EngineId");

            migrationBuilder.CreateIndex(
                name: "IX_CarModifications_CarModelId",
                table: "CarModifications",
                column: "CarModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CarModifications_TenantId",
                table: "CarModifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CarModifications_TransmissionTypeId",
                table: "CarModifications",
                column: "TransmissionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CarModifications_WheelDriveTypeId",
                table: "CarModifications",
                column: "WheelDriveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailAlternativeRelations_AlternativeDetailId",
                table: "DetailAlternativeRelations",
                column: "AlternativeDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailOriginalRelations_OriginalDetailId",
                table: "DetailOriginalRelations",
                column: "OriginalDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_Engines_CarModelId",
                table: "Engines",
                column: "CarModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Engines_EngineTypeId",
                table: "Engines",
                column: "EngineTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Engines_TenantId",
                table: "Engines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineTypes_TenantId",
                table: "EngineTypes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TransmissionTypes_TenantId",
                table: "TransmissionTypes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_WheelDriveTypes_TenantId",
                table: "WheelDriveTypes",
                column: "TenantId");

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
                name: "FK_DetailManufactures_TenantInfoEntity_TenantId",
                table: "DetailManufactures");

            migrationBuilder.DropForeignKey(
                name: "FK_Details_DetailManufactures_DetailManufactureId",
                table: "Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Details_TenantInfoEntity_TenantId",
                table: "Details");

            migrationBuilder.DropTable(
                name: "CarModificationDetails");

            migrationBuilder.DropTable(
                name: "CarModificationEngines");

            migrationBuilder.DropTable(
                name: "DetailAlternativeRelations");

            migrationBuilder.DropTable(
                name: "DetailOriginalRelations");

            migrationBuilder.DropTable(
                name: "CarModifications");

            migrationBuilder.DropTable(
                name: "Engines");

            migrationBuilder.DropTable(
                name: "TransmissionTypes");

            migrationBuilder.DropTable(
                name: "WheelDriveTypes");

            migrationBuilder.DropTable(
                name: "EngineTypes");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "CarModels",
                newName: "NameRU");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "CarManufactures",
                newName: "NameRU");

            migrationBuilder.AddColumn<int>(
                name: "CarModelId",
                table: "Details",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NameEN",
                table: "CarModels",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "CarManufactures",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEN",
                table: "CarManufactures",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Details_CarModelId",
                table: "Details",
                column: "CarModelId");

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
                name: "FK_Details_CarModels_CarModelId",
                table: "Details",
                column: "CarModelId",
                principalTable: "CarModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Details_DetailManufactures_DetailManufactureId",
                table: "Details",
                column: "DetailManufactureId",
                principalTable: "DetailManufactures",
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
    }
}
