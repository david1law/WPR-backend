using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WPR_backend.Migrations
{
    /// <inheritdoc />
    public partial class StoreDeletedUserEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Verhuur_AspNetUsers_UserId",
                table: "Verhuur");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Verhuur",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "DeletedUserEmail",
                table: "Verhuur",
                type: "nvarchar(255)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Verhuur_AspNetUsers_UserId",
                table: "Verhuur",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Verhuur_AspNetUsers_UserId",
                table: "Verhuur");

            migrationBuilder.DropColumn(
                name: "DeletedUserEmail",
                table: "Verhuur");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Verhuur",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Verhuur_AspNetUsers_UserId",
                table: "Verhuur",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
