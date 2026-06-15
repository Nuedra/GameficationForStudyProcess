using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.DataAccess.Postgress.Migrations
{
    [DbContext(typeof(PlatformDbContext))]
    [Migration("20260615000100_AddAchievementRarityEnum")]
    public partial class AddAchievementRarityEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """UPDATE "achievements" SET "Rarity" = LOWER("Rarity");""");

            migrationBuilder.AlterColumn<string>(
                name: "Rarity",
                table: "achievements",
                type: "text",
                nullable: false,
                defaultValue: "common",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Common");

            migrationBuilder.AddCheckConstraint(
                name: "CK_achievements_Rarity",
                table: "achievements",
                sql: "\"Rarity\" IN ('common', 'rare', 'epic', 'legendary')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_achievements_Rarity",
                table: "achievements");

            migrationBuilder.Sql(
                """
                UPDATE "achievements"
                SET "Rarity" = UPPER(LEFT("Rarity", 1)) || SUBSTRING("Rarity" FROM 2);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Rarity",
                table: "achievements",
                type: "text",
                nullable: false,
                defaultValue: "Common",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "common");
        }
    }
}
