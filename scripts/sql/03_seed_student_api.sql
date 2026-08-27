BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'courses'
          AND column_name = 'ContentScopeID'
    ) THEN
        RAISE EXCEPTION 'Local database schema is outdated: courses.ContentScopeID is missing. Apply scripts/sql/00_create_local_schema.sql before running 03_seed_student_api.sql.';
    END IF;
END $$;

CREATE TEMP TABLE seed_achievement_ids (
    "Id" uuid PRIMARY KEY
) ON COMMIT DROP;

INSERT INTO seed_achievement_ids ("Id")
SELECT ('00000000-0000-0000-0000-' || lpad(number::text, 12, '0'))::uuid
FROM (
    SELECT generate_series(1, 50) AS number
    UNION ALL
    SELECT generate_series(52, 55) AS number
) AS template_ids;

DELETE FROM "achievement_connections"
WHERE "SourceId" IN (SELECT "Id" FROM seed_achievement_ids)
   OR "TargetId" IN (SELECT "Id" FROM seed_achievement_ids);

DELETE FROM "student_achievements"
WHERE "AchievementID" IN (SELECT "Id" FROM seed_achievement_ids);

DELETE FROM "achievement_criterias"
WHERE "AchievementID" IN (SELECT "Id" FROM seed_achievement_ids);

DELETE FROM "achievements"
WHERE "Id" IN (SELECT "Id" FROM seed_achievement_ids);

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


DELETE FROM "course_instance_teachers"
WHERE "CourseID" = 'a1000000-0000-0000-0000-000000000002'
  AND "Year" = 2026
  AND "PersonID" = 'b1000000-0000-0000-0000-000000000001';

INSERT INTO "course_instance_teachers"
    ("CourseID", "Year", "PersonID", "StartDate", "EndDate", "IsLead")
VALUES
(
    'a1000000-0000-0000-0000-000000000001',
    2026,
    'b1000000-0000-0000-0000-000000000001',
    TIMESTAMPTZ '2026-01-01T00:00:00Z',
    NULL,
    TRUE
)
ON CONFLICT ("CourseID", "Year", "PersonID") DO UPDATE SET
    "StartDate" = EXCLUDED."StartDate",
    "EndDate" = EXCLUDED."EndDate",
    "IsLead" = EXCLUDED."IsLead";

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

DELETE FROM "course_instance_students"
WHERE "CourseID" = 'a1000000-0000-0000-0000-000000000002'
  AND "Year" = 2026
  AND "PersonID" IN (
      SELECT ('b0000000-0000-0000-0000-' || lpad(number::text, 12, '0'))::uuid
      FROM generate_series(1, 12) AS number
  );

INSERT INTO "course_instance_students"
    ("CourseID", "Year", "PersonID", "StartDate", "EndDate")
SELECT
    'a1000000-0000-0000-0000-000000000002',
    2026,
    ('b0000000-0000-0000-0000-' || lpad(number::text, 12, '0'))::uuid,
    TIMESTAMPTZ '2026-02-01T00:00:00Z',
    NULL
FROM (
    SELECT generate_series(13, 20) AS number
) AS enrolled
ON CONFLICT ("CourseID", "Year", "PersonID") DO UPDATE SET
    "StartDate" = EXCLUDED."StartDate",
    "EndDate" = EXCLUDED."EndDate";

CREATE TEMP TABLE seed_template_achievements (
    "Number" integer PRIMARY KEY,
    "Title" text NOT NULL
) ON COMMIT DROP;

INSERT INTO seed_template_achievements ("Number", "Title")
VALUES
    (1, 'Подмастерье'),
    (2, 'Первый коммит'),
    (3, 'Полпути пройдено!'),
    (4, 'Лабораторный ветеран'),
    (5, 'Первое появление'),
    (6, 'Уверенный слушатель'),
    (7, 'Образцовый студент'),
    (8, 'О, массив!'),
    (9, 'Лист, покажись'),
    (10, 'Секвенсор'),
    (11, 'Сегментатор-3000'),
    (12, 'Подвинься!'),
    (13, 'Дайте приоритет!'),
    (14, 'Деконструктор'),
    (15, 'СегменДек'),
    (16, 'Чистый разум'),
    (17, 'Двуликий Янус'),
    (18, 'Алгебраист'),
    (19, 'Магистр порядка'),
    (20, 'Перегруженный'),
    (21, 'Генеральный шаблон'),
    (22, 'Сцепщик'),
    (23, 'Расщепитель'),
    (24, 'Инвертни это!'),
    (25, 'MapReduce: давай заново'),
    (26, 'Просеял — победил'),
    (27, 'Фильтруй — не тормози'),
    (28, 'Башня для продвинутых'),
    (29, 'Кучник'),
    (30, 'B-древовод'),
    (31, 'Балансировщик'),
    (32, 'Полупорядоченный'),
    (33, 'Хешер'),
    (34, 'Автоперестройка'),
    (35, 'Древолаз'),
    (36, 'Кусок графика'),
    (37, 'Путеискатель'),
    (38, 'Граф-теоретик'),
    (39, 'Без утечек'),
    (40, 'Аналитик функций'),
    (41, 'Логик порядка'),
    (42, 'Властелин рекурсии'),
    (43, 'Повелитель линейных структур'),
    (44, 'Хранитель памяти'),
    (45, 'Инженер многомерных структур'),
    (46, 'Очередник'),
    (47, 'Мастер базы'),
    (48, 'Азбука теории'),
    (49, 'Первый опыт'),
    (50, 'Полпути пройдено!'),
    (52, 'Итоги'),
    (53, 'Мастер курса'),
    (54, 'Ас курса'),
    (55, 'Главный лентяй курса');

INSERT INTO "achievements"
    ("Id", "Title", "Description", "Year", "Rarity", "Track", "LabID", "CourseID")
SELECT
    ('00000000-0000-0000-0000-' || lpad("Number"::text, 12, '0'))::uuid,
    "Title",
    'Демо-ачивка из XML-шаблона графа достижений',
    2026,
    'common',
    'achievement-graph-template',
    NULL,
    'a1000000-0000-0000-0000-000000000001'::uuid
FROM seed_template_achievements
ON CONFLICT ("Id") DO UPDATE SET
    "Title" = EXCLUDED."Title",
    "Description" = EXCLUDED."Description",
    "Year" = EXCLUDED."Year",
    "Rarity" = EXCLUDED."Rarity",
    "Track" = EXCLUDED."Track",
    "LabID" = EXCLUDED."LabID",
    "CourseID" = EXCLUDED."CourseID";

INSERT INTO "achievement_criterias" ("Id", "IsEnabled", "Expression", "AchievementID")
SELECT
    ('f0000000-0000-0000-0000-' || lpad("Number"::text, 12, '0'))::uuid,
    TRUE,
    'template_achievement_' || "Number",
    ('00000000-0000-0000-0000-' || lpad("Number"::text, 12, '0'))::uuid
FROM seed_template_achievements
ON CONFLICT ("AchievementID") DO UPDATE SET
    "IsEnabled" = EXCLUDED."IsEnabled",
    "Expression" = EXCLUDED."Expression";

CREATE TEMP TABLE seed_template_edges (
    "Ordinal" integer PRIMARY KEY,
    "SourceNumber" integer NOT NULL,
    "TargetNumber" integer NOT NULL
) ON COMMIT DROP;

INSERT INTO seed_template_edges ("Ordinal", "SourceNumber", "TargetNumber")
VALUES
    (1, 1, 2),
    (2, 1, 5),
    (3, 1, 48),
    (4, 1, 8),
    (5, 1, 12),
    (6, 1, 16),
    (7, 1, 20),
    (8, 1, 24),
    (9, 1, 28),
    (10, 1, 32),
    (11, 1, 36),
    (12, 3, 4),
    (13, 4, 52),
    (14, 5, 6),
    (15, 6, 7),
    (16, 7, 52),
    (17, 48, 49),
    (18, 49, 50),
    (19, 50, 52),
    (20, 8, 9),
    (21, 9, 10),
    (22, 10, 11),
    (23, 11, 47),
    (24, 12, 13),
    (25, 13, 14),
    (26, 14, 15),
    (27, 15, 46),
    (28, 16, 17),
    (29, 17, 18),
    (30, 18, 19),
    (31, 19, 45),
    (32, 20, 21),
    (33, 21, 22),
    (34, 22, 23),
    (35, 23, 44),
    (36, 24, 25),
    (37, 25, 26),
    (38, 26, 27),
    (39, 27, 43),
    (40, 28, 29),
    (41, 29, 30),
    (42, 30, 31),
    (43, 31, 42),
    (44, 32, 33),
    (45, 33, 34),
    (46, 34, 35),
    (47, 35, 41),
    (48, 36, 37),
    (49, 37, 38),
    (50, 38, 39),
    (51, 39, 40),
    (52, 40, 52),
    (53, 41, 52),
    (54, 42, 52),
    (55, 43, 52),
    (56, 44, 52),
    (57, 45, 52),
    (58, 46, 52),
    (59, 47, 52),
    (60, 52, 53),
    (61, 52, 54),
    (62, 52, 55),
    (63, 2, 3);

INSERT INTO "achievement_connections" ("Id", "SourceId", "TargetId")
SELECT
    ('92000000-0000-0000-0000-' || lpad("Ordinal"::text, 12, '0'))::uuid,
    ('00000000-0000-0000-0000-' || lpad("SourceNumber"::text, 12, '0'))::uuid,
    ('00000000-0000-0000-0000-' || lpad("TargetNumber"::text, 12, '0'))::uuid
FROM seed_template_edges
ON CONFLICT ("SourceId", "TargetId") DO UPDATE SET
    "Id" = EXCLUDED."Id";

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
    '00000000-0000-0000-0000-000000000001',
    'b0000000-0000-0000-0000-000000000001'
),
(
    '91000000-0000-0000-0000-000000000002',
    TIMESTAMPTZ '2026-04-01T10:00:00Z',
    TIMESTAMPTZ '2026-04-01T10:05:00Z',
    FALSE,
    FALSE,
    NULL,
    '00000000-0000-0000-0000-000000000002',
    'b0000000-0000-0000-0000-000000000001'
),
(
    '91000000-0000-0000-0000-000000000003',
    TIMESTAMPTZ '2026-03-05T10:00:00Z',
    TIMESTAMPTZ '2026-03-05T10:05:00Z',
    FALSE,
    FALSE,
    NULL,
    '00000000-0000-0000-0000-000000000001',
    'b0000000-0000-0000-0000-000000000002'
)
ON CONFLICT ("StudentID", "AchievementID") DO UPDATE SET
    "AchievementGotDate" = EXCLUDED."AchievementGotDate",
    "AchievementFoundDate" = EXCLUDED."AchievementFoundDate";

COMMIT;
