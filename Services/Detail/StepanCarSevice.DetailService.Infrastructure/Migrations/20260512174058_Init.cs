using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StepanCarSevice.DetailService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
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

            migrationBuilder.CreateTable(
                name: "CarManufactures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NameEN = table.Column<string>(type: "text", nullable: false),
                    NameRU = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarManufactures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarManufactures_TenantInfoEntity_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfoEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetailManufactures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailManufactures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailManufactures_TenantInfoEntity_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfoEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ManufactureId = table.Column<int>(type: "integer", nullable: false),
                    NameEN = table.Column<string>(type: "text", nullable: false),
                    NameRU = table.Column<string>(type: "text", nullable: false),
                    YearFrom = table.Column<int>(type: "integer", nullable: false),
                    YearTo = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarModels_CarManufactures_ManufactureId",
                        column: x => x.ManufactureId,
                        principalTable: "CarManufactures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarModels_TenantInfoEntity_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfoEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Details",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    OriginalCode = table.Column<string>(type: "text", nullable: false),
                    DetailManufactureId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CarModelId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Details_CarModels_CarModelId",
                        column: x => x.CarModelId,
                        principalTable: "CarModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Details_DetailManufactures_DetailManufactureId",
                        column: x => x.DetailManufactureId,
                        principalTable: "DetailManufactures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Details_TenantInfoEntity_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfoEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarManufactures_TenantId",
                table: "CarManufactures",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CarModels_ManufactureId",
                table: "CarModels",
                column: "ManufactureId");

            migrationBuilder.CreateIndex(
                name: "IX_CarModels_TenantId",
                table: "CarModels",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailManufactures_TenantId",
                table: "DetailManufactures",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Details_CarModelId",
                table: "Details",
                column: "CarModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Details_DetailManufactureId",
                table: "Details",
                column: "DetailManufactureId");

            migrationBuilder.CreateIndex(
                name: "IX_Details_TenantId",
                table: "Details",
                column: "TenantId");

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
                name: "Details");

            migrationBuilder.DropTable(
                name: "CarModels");

            migrationBuilder.DropTable(
                name: "DetailManufactures");

            migrationBuilder.DropTable(
                name: "CarManufactures");

            migrationBuilder.DropTable(
                name: "TenantInfoEntity");
        }
    }
}
