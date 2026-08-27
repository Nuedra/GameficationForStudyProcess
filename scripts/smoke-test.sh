#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

# shellcheck disable=SC1091
source scripts/local-env.sh

for required_command in curl docker dotnet npm; do
    if ! command -v "$required_command" >/dev/null 2>&1; then
        echo "Missing required command: $required_command" >&2
        exit 1
    fi
done

temporary_directory="$(mktemp -d "${TMPDIR:-/tmp}/nir-smoke.XXXXXX")"
response_body="$temporary_directory/response-body"
application_log="$temporary_directory/application.log"
student_cookie="$temporary_directory/student-cookie.txt"
restricted_student_cookie="$temporary_directory/restricted-student-cookie.txt"
teacher_cookie="$temporary_directory/teacher-cookie.txt"
administrator_cookie="$temporary_directory/administrator-cookie.txt"
application_pid=""
base_url="http://localhost:5284"

cleanup() {
    local exit_code=$?
    trap - EXIT INT TERM

    if [[ -n "$application_pid" ]] && kill -0 "$application_pid" >/dev/null 2>&1; then
        kill "$application_pid" >/dev/null 2>&1 || true
        wait "$application_pid" >/dev/null 2>&1 || true
    fi

    if (( exit_code != 0 )) && [[ -f "$application_log" ]]; then
        echo "Last application log lines:" >&2
        tail -n 80 "$application_log" >&2
    fi

    rm -rf "$temporary_directory"
    exit "$exit_code"
}

trap cleanup EXIT INT TERM

fail() {
    echo "Smoke test failed: $1" >&2
    exit 1
}

request() {
    local status
    status="$(curl --silent --show-error --output "$response_body" --write-out "%{http_code}" "$@")" ||
        fail "request could not be completed"
    printf '%s' "$status"
}

expect_status() {
    local expected_status="$1"
    shift

    local actual_status
    actual_status="$(request "$@")"
    [[ "$actual_status" == "$expected_status" ]] ||
        fail "expected HTTP $expected_status, got $actual_status"
}

expect_body_contains() {
    local expected_text="$1"
    grep -Fq "$expected_text" "$response_body" ||
        fail "response does not contain: $expected_text"
}

expect_body_not_contains() {
    local unexpected_text="$1"
    if grep -Fq "$unexpected_text" "$response_body"; then
        fail "response unexpectedly contains: $unexpected_text"
    fi
}

get_csrf_token() {
    local cookie_file="$1"
    if [[ ! -f "$cookie_file" ]]; then
        : > "$cookie_file"
    fi
    expect_status 200 \
        --cookie "$cookie_file" \
        --cookie-jar "$cookie_file" \
        "$base_url/api/auth/csrf"

    local token
    token="$(sed -n 's/.*"token":"\([^"]*\)".*/\1/p' "$response_body")"
    [[ -n "$token" ]] || fail "CSRF response does not contain a token"
    printf '%s' "$token"
}

expect_node_state() {
    local achievement_id="$1"
    local expected_state="$2"

    sed -n "/AchivementId=\"$achievement_id\"/,/<\\/node>/p" "$response_body" |
        grep -Fq "<status state=\"$expected_state\"" ||
        fail "achievement $achievement_id does not have state $expected_state"
}

wait_for_readiness() {
    local attempts=45

    while (( attempts > 0 )); do
        local status
        status="$(curl --silent --output "$response_body" --write-out "%{http_code}" \
            "$base_url/health/ready" 2>/dev/null || true)"

        if [[ "$status" == "200" ]] && grep -Fq '"status":"ready"' "$response_body"; then
            return
        fi

        attempts=$((attempts - 1))
        sleep 1
    done

    fail "application did not become ready within 45 seconds"
}

echo "Preparing local database..."
./scripts/local-setup.sh

echo "Starting application..."
dotnet run \
    --project "Platform.Application/Platform.Application.csproj" \
    --launch-profile http \
    >"$application_log" 2>&1 &
application_pid=$!

wait_for_readiness

student_id="b0000000-0000-0000-0000-000000000001"
second_student_id="b0000000-0000-0000-0000-000000000002"
restricted_student_id="b0000000-0000-0000-0000-000000000006"
teacher_id="b1000000-0000-0000-0000-000000000001"
administrator_id="b2000000-0000-0000-0000-000000000001"
course_id="a1000000-0000-0000-0000-000000000001"
foreign_course_id="a1000000-0000-0000-0000-000000000002"
missing_course_id="a1000000-0000-0000-0000-000000000099"
achievement_one_id="00000000-0000-0000-0000-000000000001"
achievement_three_id="00000000-0000-0000-0000-000000000003"

echo "Checking unauthenticated and invalid-login responses..."
expect_status 401 "$base_url/api/auth/me"
expect_status 401 "$base_url/api/student/courses"
expect_status 401 "$base_url/api/staff/courses"
expect_status 400 \
    --header "Content-Type: application/json" \
    --data "{\"id\":\"$student_id\"}" \
    "$base_url/api/auth/login"
student_csrf_token="$(get_csrf_token "$student_cookie")"
expect_status 400 \
    --header "Content-Type: application/json" \
    --header "X-CSRF-TOKEN: $student_csrf_token" \
    --cookie "$student_cookie" \
    --data '{"id":"00000000-0000-0000-0000-000000000000"}' \
    "$base_url/api/auth/login"

echo "Checking the authenticated student flow..."
expect_status 200 \
    --cookie "$student_cookie" \
    --cookie-jar "$student_cookie" \
    --header "Content-Type: application/json" \
    --header "X-CSRF-TOKEN: $student_csrf_token" \
    --data "{\"id\":\"$student_id\"}" \
    "$base_url/api/auth/login"
expect_body_contains "$student_id"

student_csrf_token="$(get_csrf_token "$student_cookie")"

expect_status 200 --cookie "$student_cookie" "$base_url/api/auth/me"
expect_body_contains "$student_id"

expect_status 200 --cookie "$student_cookie" "$base_url/api/student/courses"
expect_body_contains "$course_id"
expect_status 403 --cookie "$student_cookie" "$base_url/api/staff/courses"
expect_body_contains '"code":"access_denied"'

expect_status 200 \
    --cookie "$student_cookie" \
    --header "Accept: application/xml" \
    "$base_url/api/student/courses/$course_id/2026/achievements/graph"
expect_node_state "$achievement_three_id" "available"

expect_status 200 \
    --cookie "$student_cookie" \
    --header "Accept: application/xml" \
    --header "X-CSRF-TOKEN: $student_csrf_token" \
    --request POST \
    "$base_url/api/student/courses/$course_id/2026/achievements/graph/refresh"
expect_node_state "$achievement_three_id" "earned"

expect_status 200 \
    --cookie "$student_cookie" \
    --header "Accept: application/xml" \
    --header "X-CSRF-TOKEN: $student_csrf_token" \
    --request POST \
    "$base_url/api/student/courses/$course_id/2026/achievements/graph/refresh"
expect_node_state "$achievement_three_id" "earned"

echo "Checking 404 and 403 responses..."
expect_status 404 \
    --cookie "$student_cookie" \
    "$base_url/api/student/courses/$missing_course_id/2026/achievements/graph"

expect_status 204 \
    --cookie "$student_cookie" \
    --cookie-jar "$student_cookie" \
    --header "X-CSRF-TOKEN: $student_csrf_token" \
    --request POST \
    "$base_url/api/auth/logout"
expect_status 401 --cookie "$student_cookie" "$base_url/api/auth/me"

expect_status 200 \
    --cookie "$restricted_student_cookie" \
    --cookie-jar "$restricted_student_cookie" \
    --header "Content-Type: application/json" \
    --header "X-CSRF-TOKEN: $(get_csrf_token "$restricted_student_cookie")" \
    --data "{\"id\":\"$restricted_student_id\"}" \
    "$base_url/api/auth/login"
expect_status 403 \
    --cookie "$restricted_student_cookie" \
    "$base_url/api/student/courses/$foreign_course_id/2026/achievements/graph"

echo "Checking teacher and administrator roles..."
expect_status 200 \
    --cookie "$teacher_cookie" \
    --cookie-jar "$teacher_cookie" \
    --header "Content-Type: application/json" \
    --header "X-CSRF-TOKEN: $(get_csrf_token "$teacher_cookie")" \
    --data "{\"id\":\"$teacher_id\"}" \
    "$base_url/api/auth/login"
expect_body_contains '"role":"teacher"'
expect_status 403 --cookie "$teacher_cookie" "$base_url/api/student/courses"
expect_body_contains '"code":"access_denied"'
expect_status 200 --cookie "$teacher_cookie" "$base_url/api/staff/courses"
expect_body_contains "$course_id"
expect_body_not_contains "$foreign_course_id"
expect_status 200 \
    --cookie "$teacher_cookie" \
    "$base_url/api/staff/courses/$course_id/2026"
expect_status 403 \
    --cookie "$teacher_cookie" \
    "$base_url/api/staff/courses/$foreign_course_id/2026"
expect_body_contains '"code":"course_access_denied"'

echo "Checking achievement award listing, revocation and re-awarding..."
teacher_csrf_token="$(get_csrf_token "$teacher_cookie")"
expect_status 200 \
    --cookie "$teacher_cookie" \
    "$base_url/api/staff/courses/$course_id/2026/achievements/$achievement_one_id/awards"
expect_body_contains "$student_id"
expect_body_contains "$second_student_id"

expect_status 200 \
    --cookie "$teacher_cookie" \
    --header "X-CSRF-TOKEN: $teacher_csrf_token" \
    --request DELETE \
    "$base_url/api/staff/courses/$course_id/2026/achievements/$achievement_three_id/awards/$student_id"
expect_body_contains '"awardCount":0'

student_csrf_token="$(get_csrf_token "$student_cookie")"
expect_status 200 \
    --cookie "$student_cookie" \
    --cookie-jar "$student_cookie" \
    --header "Content-Type: application/json" \
    --header "X-CSRF-TOKEN: $student_csrf_token" \
    --data "{\"id\":\"$student_id\"}" \
    "$base_url/api/auth/login"
student_csrf_token="$(get_csrf_token "$student_cookie")"
expect_status 200 \
    --cookie "$student_cookie" \
    --header "Accept: application/xml" \
    --header "X-CSRF-TOKEN: $student_csrf_token" \
    --request POST \
    "$base_url/api/student/courses/$course_id/2026/achievements/graph/refresh"
expect_node_state "$achievement_three_id" "earned"

expect_status 200 \
    --cookie "$administrator_cookie" \
    --cookie-jar "$administrator_cookie" \
    --header "Content-Type: application/json" \
    --header "X-CSRF-TOKEN: $(get_csrf_token "$administrator_cookie")" \
    --data "{\"id\":\"$administrator_id\"}" \
    "$base_url/api/auth/login"
expect_body_contains '"role":"administrator"'
expect_status 200 --cookie "$administrator_cookie" "$base_url/api/auth/session"
expect_body_contains '"sessionId"'
expect_status 200 --cookie "$administrator_cookie" "$base_url/api/staff/courses"
expect_body_contains "$course_id"
expect_body_contains "$foreign_course_id"
expect_status 200 \
    --cookie "$administrator_cookie" \
    "$base_url/api/staff/courses/$foreign_course_id/2026"

echo "Checking that the Vue graph component accepts XML..."
npm ci --prefix "Graph.Component"
npm --prefix "Graph.Component" test -- --run

echo "Smoke scenario passed."
