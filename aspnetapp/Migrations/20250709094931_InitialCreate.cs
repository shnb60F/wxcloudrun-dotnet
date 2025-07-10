using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserXP = table.Column<int>(type: "int", nullable: false),
                    UserLevel = table.Column<short>(type: "smallint", nullable: false),
                    UserGold = table.Column<long>(type: "bigint", nullable: false),
                    UserAP = table.Column<short>(type: "smallint", nullable: false),
                    UserCreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UniqueId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Guarders",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GuarderType = table.Column<int>(type: "int", nullable: false),
                    GuarderLevel = table.Column<short>(type: "smallint", nullable: false),
                    GuarderKakera = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guarders", x => new { x.UserId, x.GuarderType });
                    table.ForeignKey(
                        name: "FK_Guarders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuestionID = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuestionStar = table.Column<short>(type: "smallint", nullable: false),
                    QuestionBestTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => new { x.UserId, x.QuestionID });
                    table.ForeignKey(
                        name: "FK_Questions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TreasureBoxs",
                columns: table => new
                {
                    TreasureBoxId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TreasureBoxType = table.Column<int>(type: "int", nullable: false),
                    TreasureBoxCreateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreasureBoxs", x => x.TreasureBoxId);
                    table.ForeignKey(
                        name: "FK_TreasureBoxs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TreasureBoxs_UserId",
                table: "TreasureBoxs",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Guarders");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "TreasureBoxs");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
