-- Временная локальная схема до появления настоящей БД LMS.
-- Это не EF migration: скрипт предназначен только для disposable Docker-базы.

CREATE TABLE IF NOT EXISTS "courses" (
    "Id" uuid PRIMARY KEY,
    "Title" text NOT NULL,
    "Description" text NULL,
    "AuthorEntity" text NULL,
    "ContentScopeID" uuid NULL,
    "PreviousID" uuid NULL REFERENCES "courses" ("Id") ON DELETE SET NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_courses_ContentScopeID"
    ON "courses" ("ContentScopeID");
CREATE INDEX IF NOT EXISTS "IX_courses_PreviousID"
    ON "courses" ("PreviousID");

CREATE TABLE IF NOT EXISTS "students" (
    "Id" uuid PRIMARY KEY,
    "Name" text NOT NULL,
    "Surname" text NOT NULL,
    "Group" text NOT NULL
);

CREATE TABLE IF NOT EXISTS "achievements" (
    "Id" uuid PRIMARY KEY,
    "Title" text NOT NULL,
    "Description" text NULL,
    "Year" integer NOT NULL,
    "Rarity" text NOT NULL DEFAULT 'common'
        CHECK ("Rarity" IN ('common', 'rare', 'epic', 'legendary')),
    "Track" text NOT NULL DEFAULT 'default',
    "LabID" uuid NULL,
    "CourseID" uuid NOT NULL
);

-- Older EXT-03.2 revisions created this index before legacy demo data was
-- aligned. Drop it during bootstrap so existing local databases can be
-- recreated idempotently; title uniqueness is enforced by the application
-- service for new and edited achievements.
DROP INDEX IF EXISTS "IX_achievements_CourseID_Year_Title";

CREATE INDEX IF NOT EXISTS "IX_achievements_CourseID"
    ON "achievements" ("CourseID");
CREATE INDEX IF NOT EXISTS "IX_achievements_LabID"
    ON "achievements" ("LabID");

CREATE TABLE IF NOT EXISTS "course_instances" (
    "CourseID" uuid NOT NULL,
    "Year" integer NOT NULL,
    "ContentScopeID" uuid NOT NULL,
    "CreatedAt" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY ("CourseID", "Year"),
    FOREIGN KEY ("CourseID") REFERENCES "courses" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_course_instances_ContentScopeID"
    ON "course_instances" ("ContentScopeID");

-- Временная локальная проекция Course.CourseInstanceTeachers из концептуальной
-- LMS-схемы. PersonID намеренно не ссылается на students: преподаватели в
-- текущем demo-режиме разрешаются из конфигурации, а не из таблицы людей.
CREATE TABLE IF NOT EXISTS "course_instance_teachers" (
    "CourseID" uuid NOT NULL,
    "Year" integer NOT NULL,
    "PersonID" uuid NOT NULL,
    "StartDate" timestamptz NOT NULL,
    "EndDate" timestamptz NULL,
    "IsLead" boolean NOT NULL DEFAULT FALSE,
    PRIMARY KEY ("CourseID", "Year", "PersonID"),
    FOREIGN KEY ("CourseID", "Year")
        REFERENCES "course_instances" ("CourseID", "Year") ON DELETE CASCADE,
    CHECK ("EndDate" IS NULL OR "StartDate" < "EndDate")
);

CREATE INDEX IF NOT EXISTS "IX_course_instance_teachers_PersonID"
    ON "course_instance_teachers" ("PersonID");

CREATE TABLE IF NOT EXISTS "educational_groups" (
    "GroupName" text PRIMARY KEY,
    "GroupCaption" text NOT NULL,
    "EdProgramID" uuid NOT NULL,
    "AdmissionYear" integer NOT NULL,
    "StartDate" timestamptz NOT NULL,
    "EndDate" timestamptz NULL
);

CREATE TABLE IF NOT EXISTS "course_instance_students" (
    "CourseID" uuid NOT NULL,
    "Year" integer NOT NULL,
    "PersonID" uuid NOT NULL,
    "StartDate" timestamptz NOT NULL,
    "EndDate" timestamptz NULL,
    PRIMARY KEY ("CourseID", "Year", "PersonID"),
    FOREIGN KEY ("CourseID", "Year")
        REFERENCES "course_instances" ("CourseID", "Year") ON DELETE CASCADE,
    FOREIGN KEY ("PersonID") REFERENCES "students" ("Id") ON DELETE CASCADE,
    CHECK ("EndDate" IS NULL OR "StartDate" < "EndDate")
);

CREATE INDEX IF NOT EXISTS "IX_course_instance_students_PersonID"
    ON "course_instance_students" ("PersonID");

CREATE TABLE IF NOT EXISTS "group_students" (
    "PersonID" uuid NOT NULL,
    "EdGroupID" text NOT NULL,
    "StartDate" timestamptz NOT NULL,
    "EndDate" timestamptz NULL,
    PRIMARY KEY ("PersonID", "EdGroupID", "StartDate"),
    FOREIGN KEY ("PersonID") REFERENCES "students" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("EdGroupID") REFERENCES "educational_groups" ("GroupName") ON DELETE CASCADE,
    CHECK ("EndDate" IS NULL OR "StartDate" < "EndDate")
);

CREATE INDEX IF NOT EXISTS "IX_group_students_EdGroupID"
    ON "group_students" ("EdGroupID");

CREATE TABLE IF NOT EXISTS "achievement_connections" (
    "Id" uuid PRIMARY KEY,
    "SourceId" uuid NOT NULL REFERENCES "achievements" ("Id") ON DELETE RESTRICT,
    "TargetId" uuid NOT NULL REFERENCES "achievements" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_achievement_connections_SourceId_TargetId"
    ON "achievement_connections" ("SourceId", "TargetId");

CREATE TABLE IF NOT EXISTS "achievement_criterias" (
    "Id" uuid PRIMARY KEY,
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "Expression" text NOT NULL,
    "Scope" text NOT NULL DEFAULT 'SameMark'
        CHECK ("Scope" IN ('SameMark', 'AcrossCourse', 'AllLabs')),
    "AchievementID" uuid NOT NULL UNIQUE
        REFERENCES "achievements" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "student_achievements" (
    "Id" uuid PRIMARY KEY,
    "AchievementGotDate" timestamptz NOT NULL,
    "AchievementFoundDate" timestamptz NOT NULL,
    "IsNotificationSeen" boolean NOT NULL DEFAULT FALSE,
    "IsFirstAnimationShown" boolean NOT NULL DEFAULT FALSE,
    "LabID" uuid NULL,
    "AchievementID" uuid NOT NULL REFERENCES "achievements" ("Id") ON DELETE CASCADE,
    "StudentID" uuid NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_student_achievements_AchievementID"
    ON "student_achievements" ("AchievementID");
CREATE INDEX IF NOT EXISTS "IX_student_achievements_LabID"
    ON "student_achievements" ("LabID");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_student_achievements_StudentID_AchievementID"
    ON "student_achievements" ("StudentID", "AchievementID");

CREATE TABLE IF NOT EXISTS "achievement_award_audit_events" (
    "Id" uuid PRIMARY KEY,
    "AwardID" uuid NOT NULL,
    "EventType" text NOT NULL
        CHECK ("EventType" IN ('Granted', 'Revoked')),
    "OccurredAt" timestamptz NOT NULL,
    "AwardedAt" timestamptz NOT NULL,
    "StudentID" uuid NOT NULL,
    "AchievementID" uuid NOT NULL,
    "AchievementTitle" text NOT NULL,
    "CourseID" uuid NOT NULL,
    "Year" integer NOT NULL,
    "ActorID" uuid NULL,
    "ActorRole" text NOT NULL
        CHECK ("ActorRole" IN ('System', 'Teacher', 'Administrator')),
    "Reason" text NOT NULL
        CHECK ("Reason" IN ('CriteriaMatched', 'ManualRevocation', 'AchievementDeletion')),
    "CriterionExpression" text NULL,
    "CriterionScope" text NULL
        CHECK ("CriterionScope" IS NULL OR "CriterionScope" IN ('SameMark', 'AcrossCourse', 'AllLabs'))
);

CREATE INDEX IF NOT EXISTS "IX_achievement_award_audit_events_CourseID_Year_OccurredAt"
    ON "achievement_award_audit_events" ("CourseID", "Year", "OccurredAt");
CREATE INDEX IF NOT EXISTS "IX_achievement_award_audit_events_StudentID_OccurredAt"
    ON "achievement_award_audit_events" ("StudentID", "OccurredAt");
CREATE INDEX IF NOT EXISTS "IX_achievement_award_audit_events_AchievementID_OccurredAt"
    ON "achievement_award_audit_events" ("AchievementID", "OccurredAt");
