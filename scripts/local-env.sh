#!/usr/bin/env bash

# This file must be sourced so that the generated connection string remains in
# the current shell: source scripts/local-env.sh
if [[ "${BASH_SOURCE[0]}" == "$0" ]]; then
    echo "Run 'source scripts/local-env.sh' from the repository root." >&2
    exit 1
fi

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
environment_file="$repository_root/.env"

if [[ ! -f "$environment_file" ]]; then
    echo "Missing .env. Create it with: cp .env.example .env" >&2
    return 1
fi

set -a
# shellcheck disable=SC1090
source "$environment_file"
set +a

: "${POSTGRES_DB:=platform}"
: "${POSTGRES_USER:=postgres}"
: "${POSTGRES_PASSWORD:=pass}"
: "${POSTGRES_PORT:=5433}"

export POSTGRES_DB POSTGRES_USER POSTGRES_PASSWORD POSTGRES_PORT
export ConnectionStrings__Platform="Host=localhost;Port=${POSTGRES_PORT};Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"
