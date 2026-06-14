using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.DataAccess.Postgress.Migrations
{
    [DbContext(typeof(PlatformDbContext))]
    [Migration("20260614000200_AddLabIdToAchievements")]
    public partial class AddLabIdToAchievements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LabID",
                table: "student_achievements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LabID",
                table: "achievements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_achievements_LabID",
                table: "student_achievements",
                column: "LabID");

            migrationBuilder.CreateIndex(
                name: "IX_achievements_LabID",
                table: "achievements",
                column: "LabID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_student_achievements_LabID",
                table: "student_achievements");

            migrationBuilder.DropIndex(
                name: "IX_achievements_LabID",
                table: "achievements");

            migrationBuilder.DropColumn(
                name: "LabID",
                table: "student_achievements");

            migrationBuilder.DropColumn(
                name: "LabID",
                table: "achievements");
        }
    }
}
