using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.DataAccess.Postgress.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseInstancesAndEducationalGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContentScopeID",
                table: "courses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "course_instances",
                columns: table => new
                {
                    CourseID = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    ContentScopeID = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_instances", x => new { x.CourseID, x.Year });
                    table.ForeignKey(
                        name: "FK_course_instances_courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "educational_groups",
                columns: table => new
                {
                    GroupName = table.Column<string>(type: "text", nullable: false),
                    GroupCaption = table.Column<string>(type: "text", nullable: false),
                    EdProgramID = table.Column<Guid>(type: "uuid", nullable: false),
                    AdmissionYear = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_educational_groups", x => x.GroupName);
                });

            migrationBuilder.CreateTable(
                name: "course_instance_students",
                columns: table => new
                {
                    CourseID = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    PersonID = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_instance_students", x => new { x.CourseID, x.Year, x.PersonID });
                    table.ForeignKey(
                        name: "FK_course_instance_students_course_instances_CourseID_Year",
                        columns: x => new { x.CourseID, x.Year },
                        principalTable: "course_instances",
                        principalColumns: new[] { "CourseID", "Year" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_instance_students_students_PersonID",
                        column: x => x.PersonID,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_students",
                columns: table => new
                {
                    PersonID = table.Column<Guid>(type: "uuid", nullable: false),
                    EdGroupID = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_students", x => new { x.PersonID, x.EdGroupID, x.StartDate });
                    table.ForeignKey(
                        name: "FK_group_students_educational_groups_EdGroupID",
                        column: x => x.EdGroupID,
                        principalTable: "educational_groups",
                        principalColumn: "GroupName",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_group_students_students_PersonID",
                        column: x => x.PersonID,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_courses_ContentScopeID",
                table: "courses",
                column: "ContentScopeID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_instance_students_PersonID",
                table: "course_instance_students",
                column: "PersonID");

            migrationBuilder.CreateIndex(
                name: "IX_course_instances_ContentScopeID",
                table: "course_instances",
                column: "ContentScopeID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_group_students_EdGroupID",
                table: "group_students",
                column: "EdGroupID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "course_instance_students");

            migrationBuilder.DropTable(
                name: "group_students");

            migrationBuilder.DropTable(
                name: "course_instances");

            migrationBuilder.DropTable(
                name: "educational_groups");

            migrationBuilder.DropIndex(
                name: "IX_courses_ContentScopeID",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "ContentScopeID",
                table: "courses");
        }
    }
}
