using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StepanCarSevice.VisitService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManufactureSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NameEN = table.Column<string>(type: "text", nullable: false),
                    NameRU = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufactureSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OwnerSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    SecondName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CarModelSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ManufactureId = table.Column<int>(type: "integer", nullable: false),
                    ManufactureSnapshotId = table.Column<int>(type: "integer", nullable: false),
                    NameEN = table.Column<string>(type: "text", nullable: false),
                    NameRU = table.Column<string>(type: "text", nullable: false),
                    YearFrom = table.Column<int>(type: "integer", nullable: false),
                    YearTo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarModelSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarModelSnapshots_ManufactureSnapshots_ManufactureSnapshotId",
                        column: x => x.ManufactureSnapshotId,
                        principalTable: "ManufactureSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    OwnerSnapshotId = table.Column<int>(type: "integer", nullable: false),
                    VIN = table.Column<string>(type: "text", nullable: false),
                    CarModelId = table.Column<int>(type: "integer", nullable: false),
                    CarModelSnapshotId = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarSnapshots_CarModelSnapshots_CarModelSnapshotId",
                        column: x => x.CarModelSnapshotId,
                        principalTable: "CarModelSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarSnapshots_OwnerSnapshots_OwnerSnapshotId",
                        column: x => x.OwnerSnapshotId,
                        principalTable: "OwnerSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetailSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CarModelId = table.Column<int>(type: "integer", nullable: false),
                    CarModelSnapshotId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailSnapshots_CarModelSnapshots_CarModelSnapshotId",
                        column: x => x.CarModelSnapshotId,
                        principalTable: "CarModelSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarId = table.Column<int>(type: "integer", nullable: false),
                    CarSnapshotId = table.Column<int>(type: "integer", nullable: false),
                    DateFrom = table.Column<string>(type: "text", nullable: true),
                    DateTo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visits_CarSnapshots_CarId",
                        column: x => x.CarId,
                        principalTable: "CarSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitId = table.Column<int>(type: "integer", nullable: false),
                    DetailId = table.Column<int>(type: "integer", nullable: false),
                    DetailSnapshotId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitDetails_DetailSnapshots_DetailSnapshotId",
                        column: x => x.DetailSnapshotId,
                        principalTable: "DetailSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisitDetails_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Works",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    VisitId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Works", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Works_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarModelSnapshots_ManufactureSnapshotId",
                table: "CarModelSnapshots",
                column: "ManufactureSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSnapshots_CarModelSnapshotId",
                table: "CarSnapshots",
                column: "CarModelSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSnapshots_OwnerSnapshotId",
                table: "CarSnapshots",
                column: "OwnerSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailSnapshots_CarModelSnapshotId",
                table: "DetailSnapshots",
                column: "CarModelSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitDetails_DetailSnapshotId",
                table: "VisitDetails",
                column: "DetailSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitDetails_VisitId",
                table: "VisitDetails",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_CarId",
                table: "Visits",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Works_VisitId",
                table: "Works",
                column: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VisitDetails");

            migrationBuilder.DropTable(
                name: "Works");

            migrationBuilder.DropTable(
                name: "DetailSnapshots");

            migrationBuilder.DropTable(
                name: "Visits");

            migrationBuilder.DropTable(
                name: "CarSnapshots");

            migrationBuilder.DropTable(
                name: "CarModelSnapshots");

            migrationBuilder.DropTable(
                name: "OwnerSnapshots");

            migrationBuilder.DropTable(
                name: "ManufactureSnapshots");
        }
    }
}
