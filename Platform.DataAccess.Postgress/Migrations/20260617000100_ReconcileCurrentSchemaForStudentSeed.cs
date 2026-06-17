using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.DataAccess.Postgress.Migrations
{
    [DbContext(typeof(PlatformDbContext))]
    [Migration("20260617000100_ReconcileCurrentSchemaForStudentSeed")]
    public partial class ReconcileCurrentSchemaForStudentSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE courses
                    ADD COLUMN IF NOT EXISTS "ContentScopeID" uuid;

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_courses_ContentScopeID"
                    ON courses ("ContentScopeID");

                ALTER TABLE achievements
                    ADD COLUMN IF NOT EXISTS "Rarity" text NOT NULL DEFAULT 'common',
                    ADD COLUMN IF NOT EXISTS "Track" text NOT NULL DEFAULT 'default',
                    ADD COLUMN IF NOT EXISTS "LabID" uuid;

                UPDATE achievements
                SET "Rarity" = LOWER("Rarity")
                WHERE "Rarity" IS NOT NULL
                  AND "Rarity" <> LOWER("Rarity");

                UPDATE achievements
                SET "Rarity" = 'common'
                WHERE "Rarity" IS NULL;

                ALTER TABLE achievements
                    ALTER COLUMN "Rarity" SET DEFAULT 'common',
                    ALTER COLUMN "Rarity" SET NOT NULL,
                    ALTER COLUMN "Track" SET DEFAULT 'default',
                    ALTER COLUMN "Track" SET NOT NULL;

                CREATE INDEX IF NOT EXISTS "IX_achievements_LabID"
                    ON achievements ("LabID");

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conrelid = 'achievements'::regclass
                          AND conname = 'CK_achievements_Rarity'
                    ) THEN
                        ALTER TABLE achievements
                            ADD CONSTRAINT "CK_achievements_Rarity"
                            CHECK ("Rarity" IN ('common', 'rare', 'epic', 'legendary'));
                    END IF;
                END $$;

                ALTER TABLE student_achievements
                    ADD COLUMN IF NOT EXISTS "LabID" uuid;

                CREATE INDEX IF NOT EXISTS "IX_student_achievements_LabID"
                    ON student_achievements ("LabID");

                ALTER TABLE achievement_criterias
                    ADD COLUMN IF NOT EXISTS "Scope" text NOT NULL DEFAULT 'SameMark';

                UPDATE achievement_criterias
                SET "Scope" = 'SameMark'
                WHERE "Scope" IS NULL;

                ALTER TABLE achievement_criterias
                    ALTER COLUMN "Scope" SET DEFAULT 'SameMark',
                    ALTER COLUMN "Scope" SET NOT NULL;

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conrelid = 'achievement_criterias'::regclass
                          AND conname = 'CK_achievement_criterias_Scope'
                    ) THEN
                        ALTER TABLE achievement_criterias
                            ADD CONSTRAINT "CK_achievement_criterias_Scope"
                            CHECK ("Scope" IN ('SameMark', 'AcrossCourse', 'AllLabs'));
                    END IF;
                END $$;

                CREATE TABLE IF NOT EXISTS course_instances (
                    "CourseID" uuid NOT NULL,
                    "Year" integer NOT NULL,
                    "ContentScopeID" uuid NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT "PK_course_instances" PRIMARY KEY ("CourseID", "Year"),
                    CONSTRAINT "FK_course_instances_courses_CourseID"
                        FOREIGN KEY ("CourseID") REFERENCES courses ("Id") ON DELETE CASCADE
                );

                ALTER TABLE course_instances
                    ADD COLUMN IF NOT EXISTS "CourseID" uuid,
                    ADD COLUMN IF NOT EXISTS "Year" integer,
                    ADD COLUMN IF NOT EXISTS "ContentScopeID" uuid,
                    ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;

                ALTER TABLE course_instances
                    ALTER COLUMN "CourseID" SET NOT NULL,
                    ALTER COLUMN "Year" SET NOT NULL,
                    ALTER COLUMN "ContentScopeID" SET NOT NULL,
                    ALTER COLUMN "CreatedAt" SET DEFAULT CURRENT_TIMESTAMP,
                    ALTER COLUMN "CreatedAt" SET NOT NULL;

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conrelid = 'course_instances'::regclass
                          AND conname = 'PK_course_instances'
                    ) THEN
                        ALTER TABLE course_instances
                            ADD CONSTRAINT "PK_course_instances" PRIMARY KEY ("CourseID", "Year");
                    END IF;
                END $$;

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conrelid = 'course_instances'::regclass
                          AND conname = 'FK_course_instances_courses_CourseID'
                    ) THEN
                        ALTER TABLE course_instances
                            ADD CONSTRAINT "FK_course_instances_courses_CourseID"
                            FOREIGN KEY ("CourseID") REFERENCES courses ("Id") ON DELETE CASCADE;
                    END IF;
                END $$;

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_course_instances_ContentScopeID"
                    ON course_instances ("ContentScopeID");

                CREATE TABLE IF NOT EXISTS educational_groups (
                    "GroupName" text NOT NULL,
                    "GroupCaption" text NOT NULL,
                    "EdProgramID" uuid NOT NULL,
                    "AdmissionYear" integer NOT NULL,
                    "StartDate" timestamp with time zone NOT NULL,
                    "EndDate" timestamp with time zone,
                    CONSTRAINT "PK_educational_groups" PRIMARY KEY ("GroupName")
                );

                ALTER TABLE educational_groups
                    ADD COLUMN IF NOT EXISTS "GroupName" text,
                    ADD COLUMN IF NOT EXISTS "GroupCaption" text,
                    ADD COLUMN IF NOT EXISTS "EdProgramID" uuid,
                    ADD COLUMN IF NOT EXISTS "AdmissionYear" integer,
                    ADD COLUMN IF NOT EXISTS "StartDate" timestamp with time zone,
                    ADD COLUMN IF NOT EXISTS "EndDate" timestamp with time zone;

                ALTER TABLE educational_groups
                    ALTER COLUMN "GroupName" SET NOT NULL,
                    ALTER COLUMN "GroupCaption" SET NOT NULL,
                    ALTER COLUMN "EdProgramID" SET NOT NULL,
                    ALTER COLUMN "AdmissionYear" SET NOT NULL,
                    ALTER COLUMN "StartDate" SET NOT NULL;

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conrelid = 'educational_groups'::regclass
                          AND conname = 'PK_educational_groups'
                    ) THEN
                        ALTER TABLE educational_groups
                            ADD CONSTRAINT "PK_educational_groups" PRIMARY KEY ("GroupName");
                    END IF;
                END $$;

                CREATE TABLE IF NOT EXISTS course_instance_students (
                    "CourseID" uuid NOT NULL,
                    "Year" integer NOT NULL,
                    "PersonID" uuid NOT NULL,
                    "StartDate" timestamp with time zone NOT NULL,
                    "EndDate" timestamp with time zone,
                    CONSTRAINT "PK_course_instance_students" PRIMARY KEY ("CourseID", "Year", "PersonID"),
                    CONSTRAINT "FK_course_instance_students_course_instances_CourseID_Year"
                        FOREIGN KEY ("CourseID", "Year") REFERENCES course_instances ("CourseID", "Year") ON DELETE CASCADE,
                    CONSTRAINT "FK_course_instance_students_students_PersonID"
                        FOREIGN KEY ("PersonID") REFERENCES students ("Id") ON DELETE CASCADE
                );

                ALTER TABLE course_instance_students
                    ADD COLUMN IF NOT EXISTS "CourseID" uuid,
                    ADD COLUMN IF NOT EXISTS "Year" integer,
                    ADD COLUMN IF NOT EXISTS "PersonID" uuid,
                    ADD COLUMN IF NOT EXISTS "StartDate" timestamp with time zone,
                    ADD COLUMN IF NOT EXISTS "EndDate" timestamp with time zone;

                ALTER TABLE course_instance_students
                    ALTER COLUMN "CourseID" SET NOT NULL,
                    ALTER COLUMN "Year" SET NOT NULL,
                    ALTER COLUMN "PersonID" SET NOT NULL,
                    ALTER COLUMN "StartDate" SET NOT NULL;

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conrelid = 'course_instance_students'::regclass
                          AND conname = 'PK_course_instance_students'
                    ) THEN
                        ALTER TABLE course_instance_students
                            ADD CONSTRAINT "PK_course_instance_students" PRIMARY KEY ("CourseID", "Year", "PersonID");
                    END IF;
                END $$;

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conrelid = 'course_instance_students'::regclass
                          AND conname = 'FK_course_instance_students_course_instances_CourseID_Year'
                    ) THEN
                        ALTER TABLE course_instance_students
                            ADD CONSTRAINT "FK_course_instance_students_course_instances_CourseID_Year"
                            FOREIGN KEY ("CourseID", "Year") REFERENCES course_instances ("CourseID", "Year") ON DELETE CASCADE;
                    END IF;
                END $$;

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conrelid = 'course_instance_students'::regclass
                          AND conname = 'FK_course_instance_students_students_PersonID'
                    ) THEN
                        ALTER TABLE course_instance_students
                            ADD CONSTRAINT "FK_course_instance_students_students_PersonID"
                            FOREIGN KEY ("PersonID") REFERENCES students ("Id") ON DELETE CASCADE;
                    END IF;
                END $$;

                CREATE INDEX IF NOT EXISTS "IX_course_instance_students_PersonID"
                    ON course_instance_students ("PersonID");

                CREATE TABLE IF NOT EXISTS group_students (
                    "PersonID" uuid NOT NULL,
                    "EdGroupID" text NOT NULL,
                    "StartDate" timestamp with time zone NOT NULL,
                    "EndDate" timestamp with time zone,
                    CONSTRAINT "PK_group_students" PRIMARY KEY ("PersonID", "EdGroupID", "StartDate"),
                    CONSTRAINT "FK_group_students_educational_groups_EdGroupID"
                        FOREIGN KEY ("EdGroupID") REFERENCES educational_groups ("GroupName") ON DELETE CASCADE,
                    CONSTRAINT "FK_group_students_students_PersonID"
                        FOREIGN KEY ("PersonID") REFERENCES students ("Id") ON DELETE CASCADE
                );

                ALTER TABLE group_students
                    ADD COLUMN IF NOT EXISTS "PersonID" uuid,
                    ADD COLUMN IF NOT EXISTS "EdGroupID" text,
                    ADD COLUMN IF NOT EXISTS "StartDate" timestamp with time zone,
                    ADD COLUMN IF NOT EXISTS "EndDate" timestamp with time zone;

                ALTER TABLE group_students
                    ALTER COLUMN "PersonID" SET NOT NULL,
                    ALTER COLUMN "EdGroupID" SET NOT NULL,
                    ALTER COLUMN "StartDate" SET NOT NULL;

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conrelid = 'group_students'::regclass
                          AND conname = 'PK_group_students'
                    ) THEN
                        ALTER TABLE group_students
                            ADD CONSTRAINT "PK_group_students" PRIMARY KEY ("PersonID", "EdGroupID", "StartDate");
                    END IF;
                END $$;

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conrelid = 'group_students'::regclass
                          AND conname = 'FK_group_students_educational_groups_EdGroupID'
                    ) THEN
                        ALTER TABLE group_students
                            ADD CONSTRAINT "FK_group_students_educational_groups_EdGroupID"
                            FOREIGN KEY ("EdGroupID") REFERENCES educational_groups ("GroupName") ON DELETE CASCADE;
                    END IF;
                END $$;

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conrelid = 'group_students'::regclass
                          AND conname = 'FK_group_students_students_PersonID'
                    ) THEN
                        ALTER TABLE group_students
                            ADD CONSTRAINT "FK_group_students_students_PersonID"
                            FOREIGN KEY ("PersonID") REFERENCES students ("Id") ON DELETE CASCADE;
                    END IF;
                END $$;

                CREATE INDEX IF NOT EXISTS "IX_group_students_EdGroupID"
                    ON group_students ("EdGroupID");
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration only repairs drift left by earlier manual migrations.
        }
    }
}
