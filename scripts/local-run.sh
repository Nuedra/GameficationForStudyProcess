#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

# shellcheck disable=SC1091
source scripts/local-env.sh

exec dotnet run \
    --project "Platform.Application/Platform.Application.csproj" \
    --launch-profile http
