using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.DataAccess.Postgress.Migrations
{
    [DbContext(typeof(PlatformDbContext))]
    [Migration("20260614000000_AddAchievementRarity")]
    public partial class AddAchievementRarity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Rarity",
                table: "achievements",
                type: "text",
                nullable: false,
                defaultValue: "Common");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rarity",
                table: "achievements");
        }
    }
}
