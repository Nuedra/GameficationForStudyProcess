-- Demo verification queries for DB task.
-- Run after scripts/sql/01_seed_demo.sql

-- 1) Student achievements with achievement and course details
SELECT
    sa."Id" AS student_achievement_id,
    sa."AchievementGotDate",
    sa."IsNotificationSeen",
    sa."LabID",
    a."Title" AS achievement_title,
    c."Title" AS course_title
FROM "student_achievements" sa
JOIN "achievements" a ON a."Id" = sa."AchievementID"
JOIN "courses" c ON c."Id" = a."CourseID"
WHERE sa."StudentID" = '11111111-1111-1111-1111-111111111111'
ORDER BY sa."AchievementGotDate" DESC;

-- 2) Course achievements with criteria
SELECT
    a."Id",
    a."Title",
    a."Year",
    a."Rarity",
    a."Track",
    a."LabID",
    ac."Expression",
    ac."IsEnabled"
FROM "achievements" a
LEFT JOIN "achievement_criterias" ac ON ac."AchievementID" = a."Id"
WHERE a."CourseID" = '22222222-2222-2222-2222-222222222222'
ORDER BY a."Year", a."Title";

-- 3) Mark notifications as seen for student
UPDATE "student_achievements"
SET "IsNotificationSeen" = TRUE
WHERE "StudentID" = '11111111-1111-1111-1111-111111111111';

-- 4) Check that update has been applied
SELECT
    "StudentID",
    COUNT(*) AS total_achievements,
    COUNT(*) FILTER (WHERE "IsNotificationSeen") AS seen_count
FROM "student_achievements"
WHERE "StudentID" = '11111111-1111-1111-1111-111111111111'
GROUP BY "StudentID";

-- 5) Verify unique constraint (StudentID + AchievementID) without breaking script
DO $$
BEGIN
    INSERT INTO "student_achievements"
        ("Id", "AchievementGotDate", "AchievementFoundDate", "IsNotificationSeen",
         "IsFirstAnimationShown", "AchievementID", "StudentID")
    VALUES
    (
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
        NOW(),
        NOW(),
        FALSE,
        FALSE,
        '33333333-3333-3333-3333-333333333333',
        '11111111-1111-1111-1111-111111111111'
    );
    RAISE EXCEPTION 'Unique constraint did not trigger as expected';
EXCEPTION
    WHEN unique_violation THEN
        RAISE NOTICE 'OK: duplicate achievement for one student is blocked by unique index';
END $$;
