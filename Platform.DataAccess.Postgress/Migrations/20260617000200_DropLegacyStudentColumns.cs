using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.DataAccess.Postgress.Migrations
{
    [DbContext(typeof(PlatformDbContext))]
    [Migration("20260617000200_DropLegacyStudentColumns")]
    public partial class DropLegacyStudentColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE students
                    DROP COLUMN IF EXISTS "StudentNumber",
                    DROP COLUMN IF EXISTS "Email";
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
