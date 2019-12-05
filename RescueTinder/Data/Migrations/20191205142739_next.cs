using Microsoft.EntityFrameworkCore.Migrations;

namespace RescueTinder.Data.Migrations
{
    public partial class next : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dogs_AspNetUsers_VetId",
                table: "Dogs");

            migrationBuilder.AlterColumn<string>(
                name: "VetId",
                table: "Dogs",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Dogs_AspNetUsers_VetId",
                table: "Dogs",
                column: "VetId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dogs_AspNetUsers_VetId",
                table: "Dogs");

            migrationBuilder.AlterColumn<string>(
                name: "VetId",
                table: "Dogs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Dogs_AspNetUsers_VetId",
                table: "Dogs",
                column: "VetId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
