using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WPR_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddVerhuurWithUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Autos",
                columns: table => new
                {
                    Kenteken = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Soort = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Merk = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Huurkosten = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Borg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Aanschafjaar = table.Column<int>(type: "int", nullable: false),
                    Kleur = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Transmissie = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Autos", x => x.Kenteken);
                });

            migrationBuilder.CreateTable(
                name: "Verhuur",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kenteken = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rijbewijs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Startdatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Einddatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ophaallocatie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Inleverlocatie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Opmerkingen = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verhuur", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Verhuur_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Verhuur_Autos_Kenteken",
                        column: x => x.Kenteken,
                        principalTable: "Autos",
                        principalColumn: "Kenteken",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Verhuur_Kenteken",
                table: "Verhuur",
                column: "Kenteken");

            migrationBuilder.CreateIndex(
                name: "IX_Verhuur_UserId",
                table: "Verhuur",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Verhuur");

            migrationBuilder.DropTable(
                name: "Autos");
        }
    }
}
