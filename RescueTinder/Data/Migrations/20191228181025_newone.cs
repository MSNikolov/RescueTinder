using Microsoft.EntityFrameworkCore.Migrations;

namespace RescueTinder.Data.Migrations
{
    public partial class newone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "VetAprovedByAdmin",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VetAprovedByAdmin",
                table: "AspNetUsers");
        }
    }
}
