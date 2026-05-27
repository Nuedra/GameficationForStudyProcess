-- Demo data for local verification after migrations.
-- Safe to rerun: cleanup by fixed IDs first.

BEGIN;

DELETE FROM "achievement_connections"
WHERE "SourceId" IN (
    '33333333-3333-3333-3333-333333333333',
    '44444444-4444-4444-4444-444444444444'
)
   OR "TargetId" IN (
    '33333333-3333-3333-3333-333333333333',
    '44444444-4444-4444-4444-444444444444'
);

DELETE FROM "student_achievements"
WHERE "StudentID" = '11111111-1111-1111-1111-111111111111'
   OR "AchievementID" IN (
       '33333333-3333-3333-3333-333333333333',
       '44444444-4444-4444-4444-444444444444'
   );

DELETE FROM "achievement_criterias"
WHERE "AchievementID" IN (
    '33333333-3333-3333-3333-333333333333',
    '44444444-4444-4444-4444-444444444444'
);

DELETE FROM "achievements"
WHERE "Id" IN (
    '33333333-3333-3333-3333-333333333333',
    '44444444-4444-4444-4444-444444444444'
);

DELETE FROM "students"
WHERE "Id" = '11111111-1111-1111-1111-111111111111';

DELETE FROM "courses"
WHERE "Id" = '22222222-2222-2222-2222-222222222222';

INSERT INTO "courses" ("Id", "Title", "Description", "AuthorEntity", "PreviousID")
VALUES (
    '22222222-2222-2222-2222-222222222222',
    'C# Fundamentals',
    'Intro course for students',
    'Teacher Demo',
    NULL
);

INSERT INTO "students" ("Id", "Name", "Surname", "Group")
VALUES (
    '11111111-1111-1111-1111-111111111111',
    'Ivan',
    'Petrov',
    'IS-101'
);

INSERT INTO "achievements" ("Id", "Title", "Description", "Year", "CourseID")
VALUES
(
    '33333333-3333-3333-3333-333333333333',
    'First Login',
    'Student opened the platform for the first time',
    2026,
    '22222222-2222-2222-2222-222222222222'
),
(
    '44444444-4444-4444-4444-444444444444',
    'First Assignment',
    'Student submitted first assignment',
    2026,
    '22222222-2222-2222-2222-222222222222'
);

INSERT INTO "achievement_criterias" ("Id", "IsEnabled", "Expression", "AchievementID")
VALUES
(
    '55555555-5555-5555-5555-555555555555',
    TRUE,
    'logins_count >= 1',
    '33333333-3333-3333-3333-333333333333'
),
(
    '66666666-6666-6666-6666-666666666666',
    TRUE,
    'assignments_submitted >= 1',
    '44444444-4444-4444-4444-444444444444'
);

INSERT INTO "student_achievements"
    ("Id", "AchievementGotDate", "AchievementFoundDate", "IsNotificationSeen",
     "IsFirstAnimationShown", "AchievementID", "StudentID")
VALUES
(
    '77777777-7777-7777-7777-777777777777',
    NOW(),
    NOW(),
    FALSE,
    FALSE,
    '33333333-3333-3333-3333-333333333333',
    '11111111-1111-1111-1111-111111111111'
),
(
    '88888888-8888-8888-8888-888888888888',
    NOW(),
    NOW(),
    FALSE,
    FALSE,
    '44444444-4444-4444-4444-444444444444',
    '11111111-1111-1111-1111-111111111111'
);

INSERT INTO "achievement_connections" ("Id", "SourceId", "TargetId")
VALUES (
    '99999999-9999-9999-9999-999999999999',
    '33333333-3333-3333-3333-333333333333',
    '44444444-4444-4444-4444-444444444444'
);

COMMIT;
