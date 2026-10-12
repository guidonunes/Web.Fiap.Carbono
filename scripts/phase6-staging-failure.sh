#!/usr/bin/env bash
# Intentionally fail staging verification, restore MongoDB, and block production.
set -euo pipefail

if [[ $# -ne 1 || "$1" != "--verify" ]]; then
  echo "Usage: $0 --verify (temporarily stops staging MongoDB; expected exit 1)" >&2
  exit 2
fi

for tool in docker bash curl jq; do
  command -v "$tool" >/dev/null || { echo "Missing command: $tool" >&2; exit 2; }
done

script_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
staging_url=http://127.0.0.1:8081/api/empresas
production_url=http://127.0.0.1:8082/api/empresas
mongodb=carbono-staging-mongodb-1
production=carbono-production-api-1
restore_needed=false
outage_verified=false

production_state() {
  docker inspect --type container \
    --format '{{.Id}} {{.Config.Image}} {{.State.StartedAt}}' "$production"
}

wait_for_database() {
  for attempt in {1..24}; do
    if [[ "$(docker inspect --type container \
      --format '{{.State.Running}} {{.State.Health.Status}}' "$mongodb")" == "true healthy" ]]; then
      return 0
    fi
    sleep 5
  done
  return 1
}

# Require a working baseline before changing the staging container's state.
[[ "$(docker inspect --type container \
  --format '{{.State.Running}} {{.State.Health.Status}}' "$mongodb")" == "true healthy" ]] || {
  echo "Staging MongoDB must already be healthy" >&2
  exit 2
}
bash "$script_dir/smoke.sh" "$staging_url" || exit 2
bash "$script_dir/smoke.sh" "$production_url" || exit 2
baseline=$(production_state) || exit 2
printf 'Verification time UTC: %s\n' "$(date -u +'%Y-%m-%dT%H:%M:%SZ')"
printf 'Production before: %s\n' "$baseline"

cleanup() {
  result=$?
  trap - EXIT INT TERM
  if [[ "$restore_needed" == "true" ]]; then
    if ! docker start "$mongodb" >/dev/null; then
      echo "ERROR: restore staging MongoDB manually with docker start $mongodb" >&2
      exit 2
    fi
    if ! wait_for_database || ! bash "$script_dir/smoke.sh" "$staging_url"; then
      echo "ERROR: staging recovery did not pass; inspect staging before continuing" >&2
      exit 2
    fi
    echo "Staging database and API recovered"
    if ! current=$(production_state); then
      echo "ERROR: could not inspect production after verification" >&2
      exit 2
    fi
    printf 'Production after: %s\n' "$current"
    if [[ "$current" != "$baseline" ]] || ! bash "$script_dir/smoke.sh" "$production_url"; then
      echo "ERROR: production changed or is unavailable" >&2
      exit 2
    fi
    echo "Production unchanged and available"
  fi
  if [[ "$outage_verified" == "true" && "$result" == "1" ]]; then
    echo "Controlled database failure verified; staging recovered; production must be skipped"
  fi
  exit "$result"
}

trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM
restore_needed=true
docker stop --timeout 15 "$mongodb" >/dev/null
[[ "$(docker inspect --type container --format '{{.State.Running}}' "$mongodb")" == "false" ]] || {
  echo "ERROR: staging MongoDB did not stop" >&2
  exit 2
}
echo "Staging MongoDB stopped; checking the real database-backed API"

if bash "$script_dir/smoke.sh" "$staging_url"; then
  echo "ERROR: staging smoke check unexpectedly passed during the database outage" >&2
  exit 2
else
  smoke_result=$?
fi
[[ "$smoke_result" == "1" ]] || exit "$smoke_result"
outage_verified=true
echo "Staging smoke check rejected the database outage"
exit 1
