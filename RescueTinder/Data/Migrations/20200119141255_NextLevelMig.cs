using Microsoft.EntityFrameworkCore.Migrations;

namespace RescueTinder.Data.Migrations
{
    public partial class NextLevelMig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Dogs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCastrated",
                table: "Dogs",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Dogs");

            migrationBuilder.DropColumn(
                name: "IsCastrated",
                table: "Dogs");
        }
    }
}
