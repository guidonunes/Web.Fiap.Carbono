#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 || -z "$1" ]]; then
  echo "Usage: $0 API_URL" >&2
  exit 1
fi

api_url="$1"

[[ "$api_url" =~ ^http://127\.0\.0\.1:[0-9]+/api/empresas$ ]] || {
  echo "Expected a host-local /api/empresas URL" >&2
  exit 1
}

for tool in curl jq; do
  command -v "$tool" >/dev/null || {
    echo "Missing command: $tool" >&2
    exit 1
  }
done

response_file=$(mktemp)
trap 'rm -f "$response_file"' EXIT

for attempt in {1..10}; do
  if status=$(curl --noproxy '*' --silent --show-error \
      --connect-timeout 2 --max-time 5 \
      --output "$response_file" \
      --write-out '%{http_code}' \
      "$api_url") \
    && [[ "$status" == "200" ]] \
    && jq -e -s \
      'length == 1 and (.[0] | type == "array" and any(.[]; .codigo == "EMP-001"))' \
      "$response_file" >/dev/null 2>&1; then
    echo "Smoke check passed: HTTP 200 and EMP-001"
    exit 0
  fi

  echo "Attempt $attempt/10 failed: HTTP ${status:-000}" >&2
  if (( attempt < 10 )); then
    sleep 2
  fi
done

echo "Smoke check failed after 10 attempts" >&2
exit 1
