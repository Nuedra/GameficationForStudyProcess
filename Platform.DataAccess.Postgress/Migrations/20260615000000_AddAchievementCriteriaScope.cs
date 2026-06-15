using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.DataAccess.Postgress.Migrations
{
    [DbContext(typeof(PlatformDbContext))]
    [Migration("20260615000000_AddAchievementCriteriaScope")]
    public partial class AddAchievementCriteriaScope : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "achievement_criterias",
                type: "text",
                nullable: false,
                defaultValue: "SameMark");

            migrationBuilder.AddCheckConstraint(
                name: "CK_achievement_criterias_Scope",
                table: "achievement_criterias",
                sql: "\"Scope\" IN ('SameMark', 'AcrossCourse', 'AllLabs')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_achievement_criterias_Scope",
                table: "achievement_criterias");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "achievement_criterias");
        }
    }
}
