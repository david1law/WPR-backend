using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WPR_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHuurkostenAndBorgToVerhuur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Borg",
                table: "Verhuur",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Huurkosten",
                table: "Verhuur",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Borg",
                table: "Verhuur");

            migrationBuilder.DropColumn(
                name: "Huurkosten",
                table: "Verhuur");
        }
    }
}
