using Microsoft.EntityFrameworkCore.Migrations;

namespace RescueTinder.Data.Migrations
{
    public partial class third : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pic_Dogs_DogId",
                table: "Pic");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pic",
                table: "Pic");

            migrationBuilder.RenameTable(
                name: "Pic",
                newName: "Pics");

            migrationBuilder.RenameIndex(
                name: "IX_Pic_DogId",
                table: "Pics",
                newName: "IX_Pics_DogId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pics",
                table: "Pics",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pics_Dogs_DogId",
                table: "Pics",
                column: "DogId",
                principalTable: "Dogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pics_Dogs_DogId",
                table: "Pics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pics",
                table: "Pics");

            migrationBuilder.RenameTable(
                name: "Pics",
                newName: "Pic");

            migrationBuilder.RenameIndex(
                name: "IX_Pics_DogId",
                table: "Pic",
                newName: "IX_Pic_DogId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pic",
                table: "Pic",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pic_Dogs_DogId",
                table: "Pic",
                column: "DogId",
                principalTable: "Dogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
