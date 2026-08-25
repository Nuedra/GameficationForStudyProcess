#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

# shellcheck disable=SC1091
source scripts/local-env.sh

docker compose up -d --wait

dotnet ef database update \
    --project "Platform.DataAccess.Postgress/Platform.DataAccess.Postgress.csproj" \
    --startup-project "Platform.DataAccess.Postgress/Platform.DataAccess.Postgress.csproj"

docker compose exec -T postgres \
    psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d "$POSTGRES_DB" \
    < "scripts/sql/03_seed_student_api.sql"

echo "Local database is ready on localhost:${POSTGRES_PORT}."
