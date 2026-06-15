-- Demo data for the student frontend API.
-- Run after applying EF Core migrations. Safe to rerun.

BEGIN;

INSERT INTO "courses"
    ("Id", "Title", "Description", "AuthorEntity", "ContentScopeID", "PreviousID")
VALUES
(
    'a1000000-0000-0000-0000-000000000001',
    'Информатика (полиморфные и абстрактные типы данных)',
    'Демонстрационное прочтение курса для студенческого API',
    'Кафедра информатики',
    'c1000000-0000-0000-0000-000000000001',
    NULL
),
(
    'a1000000-0000-0000-0000-000000000002',
    'Информатика (организация и поиск данных)',
    'Демонстрационное прочтение курса для студенческого API',
    'Кафедра информатики',
    'c1000000-0000-0000-0000-000000000002',
    NULL
)
ON CONFLICT ("Id") DO UPDATE SET
    "Title" = EXCLUDED."Title",
    "Description" = EXCLUDED."Description",
    "AuthorEntity" = EXCLUDED."AuthorEntity",
    "ContentScopeID" = EXCLUDED."ContentScopeID";

INSERT INTO "course_instances" ("CourseID", "Year", "ContentScopeID")
VALUES
(
    'a1000000-0000-0000-0000-000000000001',
    2026,
    'c2000000-0000-0000-0000-000000000001'
),
(
    'a1000000-0000-0000-0000-000000000002',
    2026,
    'c2000000-0000-0000-0000-000000000002'
)
ON CONFLICT ("CourseID", "Year") DO UPDATE SET
    "ContentScopeID" = EXCLUDED."ContentScopeID";

INSERT INTO "educational_groups"
    ("GroupName", "GroupCaption", "EdProgramID", "AdmissionYear", "StartDate", "EndDate")
VALUES
(
    'ИВТ-101',
    'ИВТ-101',
    'd1000000-0000-0000-0000-000000000001',
    2025,
    TIMESTAMPTZ '2025-09-01T00:00:00Z',
    NULL
),
(
    'ИВТ-102',
    'ИВТ-102',
    'd1000000-0000-0000-0000-000000000001',
    2025,
    TIMESTAMPTZ '2025-09-01T00:00:00Z',
    NULL
)
ON CONFLICT ("GroupName") DO UPDATE SET
    "GroupCaption" = EXCLUDED."GroupCaption",
    "EdProgramID" = EXCLUDED."EdProgramID",
    "AdmissionYear" = EXCLUDED."AdmissionYear",
    "StartDate" = EXCLUDED."StartDate",
    "EndDate" = EXCLUDED."EndDate";

INSERT INTO "students" ("Id", "Name", "Surname", "Group")
SELECT
    ('b0000000-0000-0000-0000-' || lpad(number::text, 12, '0'))::uuid,
    'Студент' || number,
    'Тестовый',
    CASE WHEN number <= 10 THEN 'ИВТ-101' ELSE 'ИВТ-102' END
FROM generate_series(1, 20) AS number
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Surname" = EXCLUDED."Surname",
    "Group" = EXCLUDED."Group";

INSERT INTO "group_students" ("PersonID", "EdGroupID", "StartDate", "EndDate")
SELECT
    ('b0000000-0000-0000-0000-' || lpad(number::text, 12, '0'))::uuid,
    CASE WHEN number <= 10 THEN 'ИВТ-101' ELSE 'ИВТ-102' END,
    TIMESTAMPTZ '2025-09-01T00:00:00Z',
    NULL
FROM generate_series(1, 20) AS number
ON CONFLICT ("PersonID", "EdGroupID", "StartDate") DO UPDATE SET
    "EndDate" = EXCLUDED."EndDate";

-- Students 1-12 are enrolled in the first course.
INSERT INTO "course_instance_students"
    ("CourseID", "Year", "PersonID", "StartDate", "EndDate")
SELECT
    'a1000000-0000-0000-0000-000000000001',
    2026,
    ('b0000000-0000-0000-0000-' || lpad(number::text, 12, '0'))::uuid,
    TIMESTAMPTZ '2026-02-01T00:00:00Z',
    NULL
FROM generate_series(1, 12) AS number
ON CONFLICT ("CourseID", "Year", "PersonID") DO UPDATE SET
    "StartDate" = EXCLUDED."StartDate",
    "EndDate" = EXCLUDED."EndDate";

-- Students 1-5 and 13-20 are enrolled in the second course.
INSERT INTO "course_instance_students"
    ("CourseID", "Year", "PersonID", "StartDate", "EndDate")
SELECT
    'a1000000-0000-0000-0000-000000000002',
    2026,
    ('b0000000-0000-0000-0000-' || lpad(number::text, 12, '0'))::uuid,
    TIMESTAMPTZ '2026-02-01T00:00:00Z',
    NULL
FROM (
    SELECT generate_series(1, 5) AS number
    UNION ALL
    SELECT generate_series(13, 20) AS number
) AS enrolled
ON CONFLICT ("CourseID", "Year", "PersonID") DO UPDATE SET
    "StartDate" = EXCLUDED."StartDate",
    "EndDate" = EXCLUDED."EndDate";

INSERT INTO "achievements"
    ("Id", "Title", "Description", "Year", "Rarity", "Track", "LabID", "CourseID")
VALUES
(
    'e1000000-0000-0000-0000-000000000001',
    'Начало работы с типами',
    'Получена первая ачивка курса',
    2026,
    'common',
    'types',
    NULL,
    'a1000000-0000-0000-0000-000000000001'
),
(
    'e1000000-0000-0000-0000-000000000002',
    'Полиморфные структуры',
    'Выполнены задания по полиморфным структурам',
    2026,
    'Rare',
    'types',
    NULL,
    'a1000000-0000-0000-0000-000000000001'
),
(
    'e1000000-0000-0000-0000-000000000003',
    'Абстрактные типы',
    'Выполнена продвинутая ветка курса',
    2026,
    'Epic',
    'abstract',
    NULL,
    'a1000000-0000-0000-0000-000000000001'
),
(
    'e2000000-0000-0000-0000-000000000001',
    'Основы поиска данных',
    'Получена первая ачивка курса',
    2026,
    'common',
    'search',
    NULL,
    'a1000000-0000-0000-0000-000000000002'
),
(
    'e2000000-0000-0000-0000-000000000002',
    'Эффективный поиск',
    'Выполнены задания по алгоритмам поиска',
    2026,
    'Rare',
    'search',
    NULL,
    'a1000000-0000-0000-0000-000000000002'
)
ON CONFLICT ("Id") DO UPDATE SET
    "Title" = EXCLUDED."Title",
    "Description" = EXCLUDED."Description",
    "Year" = EXCLUDED."Year",
    "Rarity" = EXCLUDED."Rarity",
    "Track" = EXCLUDED."Track",
    "CourseID" = EXCLUDED."CourseID";

INSERT INTO "achievement_criterias" ("Id", "IsEnabled", "Expression", "AchievementID")
VALUES
(
    'f1000000-0000-0000-0000-000000000001',
    TRUE,
    'types_start',
    'e1000000-0000-0000-0000-000000000001'
),
(
    'f1000000-0000-0000-0000-000000000002',
    TRUE,
    'polymorphism',
    'e1000000-0000-0000-0000-000000000002'
),
(
    'f1000000-0000-0000-0000-000000000003',
    TRUE,
    'abstract_types',
    'e1000000-0000-0000-0000-000000000003'
),
(
    'f2000000-0000-0000-0000-000000000001',
    TRUE,
    'search_start',
    'e2000000-0000-0000-0000-000000000001'
),
(
    'f2000000-0000-0000-0000-000000000002',
    TRUE,
    'effective_search',
    'e2000000-0000-0000-0000-000000000002'
)
ON CONFLICT ("AchievementID") DO UPDATE SET
    "IsEnabled" = EXCLUDED."IsEnabled",
    "Expression" = EXCLUDED."Expression";

INSERT INTO "student_achievements"
    ("Id", "AchievementGotDate", "AchievementFoundDate", "IsNotificationSeen",
     "IsFirstAnimationShown", "LabID", "AchievementID", "StudentID")
VALUES
(
    '91000000-0000-0000-0000-000000000001',
    TIMESTAMPTZ '2026-03-01T10:00:00Z',
    TIMESTAMPTZ '2026-03-01T10:05:00Z',
    TRUE,
    TRUE,
    NULL,
    'e1000000-0000-0000-0000-000000000001',
    'b0000000-0000-0000-0000-000000000001'
),
(
    '91000000-0000-0000-0000-000000000002',
    TIMESTAMPTZ '2026-04-01T10:00:00Z',
    TIMESTAMPTZ '2026-04-01T10:05:00Z',
    FALSE,
    FALSE,
    NULL,
    'e1000000-0000-0000-0000-000000000002',
    'b0000000-0000-0000-0000-000000000001'
),
(
    '91000000-0000-0000-0000-000000000003',
    TIMESTAMPTZ '2026-03-10T10:00:00Z',
    TIMESTAMPTZ '2026-03-10T10:05:00Z',
    FALSE,
    FALSE,
    NULL,
    'e2000000-0000-0000-0000-000000000001',
    'b0000000-0000-0000-0000-000000000001'
),
(
    '91000000-0000-0000-0000-000000000004',
    TIMESTAMPTZ '2026-03-05T10:00:00Z',
    TIMESTAMPTZ '2026-03-05T10:05:00Z',
    FALSE,
    FALSE,
    NULL,
    'e1000000-0000-0000-0000-000000000001',
    'b0000000-0000-0000-0000-000000000002'
),
(
    '91000000-0000-0000-0000-000000000005',
    TIMESTAMPTZ '2026-03-12T10:00:00Z',
    TIMESTAMPTZ '2026-03-12T10:05:00Z',
    FALSE,
    FALSE,
    NULL,
    'e2000000-0000-0000-0000-000000000001',
    'b0000000-0000-0000-0000-000000000013'
),
(
    '91000000-0000-0000-0000-000000000006',
    TIMESTAMPTZ '2026-04-12T10:00:00Z',
    TIMESTAMPTZ '2026-04-12T10:05:00Z',
    FALSE,
    FALSE,
    NULL,
    'e2000000-0000-0000-0000-000000000002',
    'b0000000-0000-0000-0000-000000000013'
)
ON CONFLICT ("StudentID", "AchievementID") DO UPDATE SET
    "AchievementGotDate" = EXCLUDED."AchievementGotDate",
    "AchievementFoundDate" = EXCLUDED."AchievementFoundDate";

COMMIT;
