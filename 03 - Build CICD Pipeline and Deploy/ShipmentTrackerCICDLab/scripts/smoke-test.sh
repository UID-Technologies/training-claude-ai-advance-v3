#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${1:?Usage: smoke-test.sh <base-url>}"

echo "Testing ${BASE_URL}/health"

for attempt in {1..12}; do
  code=$(curl -sS -o /tmp/health.txt -w "%{http_code}" "${BASE_URL}/health" || true)

  if [ "$code" = "200" ]; then
    echo "Smoke test passed."
    cat /tmp/health.txt || true
    exit 0
  fi

  echo "Attempt ${attempt}: HTTP ${code}. Retrying..."
  sleep 5
done

echo "Smoke test failed."
exit 1
