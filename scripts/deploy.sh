#!/usr/bin/env bash
set -euo pipefail

fail() {
  echo "$1" >&2
  exit 1
}

if [[ $# -ne 4 ]]; then
  fail "Usage: $0 PROJECT ENV_FILE IMAGE_DIGEST API_URL"
fi

project="$1"
env_file="$2"
image="$3"
api_url="$4"

case "$project" in
  carbono-staging)
    expected_url="http://127.0.0.1:8081/api/empresas"
    ;;
  carbono-production)
    expected_url="http://127.0.0.1:8082/api/empresas"
    ;;
  *)
    fail "Expected carbono-staging or carbono-production"
    ;;
esac

[[ "$api_url" == "$expected_url" ]] ||
  fail "API URL does not match the selected environment"

[[ -f "$env_file" && -r "$env_file" ]] ||
  fail "Environment file is missing or unreadable"

[[ "$image" =~ ^gui2506/web-fiap-carbono@sha256:[0-9a-f]{64}$ ]] ||
  fail "Expected the application repository with a full sha256 digest"

for tool in docker curl jq; do
  command -v "$tool" >/dev/null ||
    fail "Missing command: $tool"
done

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)

[[ -r "$repo_root/scripts/smoke.sh" ]] ||
  fail "Smoke script is missing"

export APP_IMAGE="$image"

compose=(
  docker compose
  --file "$repo_root/docker-compose.yml"
  --project-name "$project"
  --env-file "$env_file"
)

"${compose[@]}" config --quiet

if ! compose_images=$("${compose[@]}" config --images); then
  fail "Could not read Compose image configuration"
fi

# Read all Compose output before matching; grep -q can close a live pipe early.
if ! grep -Fxq -- "$image" <<< "$compose_images"; then
  fail "Compose did not select the supplied API image"
fi

printf 'Deploying %s with %s\n' "$project" "$image"

"${compose[@]}" pull api

"${compose[@]}" up -d \
  --no-build --pull missing \
  --wait --wait-timeout 120 \
  api

bash "$repo_root/scripts/smoke.sh" "$api_url"

echo "Deployment verified: $project"

