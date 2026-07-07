#!/usr/bin/env bash
# Test POST /api/Exams via Swagger-compatible HTTP API (curl).
# UI create exam dùng Razor Pages code-behind; script này chỉ để test API/Swagger.
#
# Usage:
#   ./backend/scripts/test-swagger-create-exam.sh
#   BASE_URL=http://localhost:5001 EXAM_SERIES_ID=1 ./backend/scripts/test-swagger-create-exam.sh

set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5001}"
EXAM_SERIES_ID="${EXAM_SERIES_ID:-1}"
TIMESTAMP="$(date +%s)"
EXAM_TITLE="Swagger Test Exam ${TIMESTAMP}"

echo "==> AcadPrep - Test Create Exam API (Swagger)"
echo "    Base URL      : ${BASE_URL}"
echo "    Exam Series ID: ${EXAM_SERIES_ID}"
echo

echo "==> [1/4] Checking Swagger is available..."
SWAGGER_STATUS="$(curl -s -o /dev/null -w "%{http_code}" "${BASE_URL}/swagger/v1/swagger.json")"
if [[ "${SWAGGER_STATUS}" != "200" ]]; then
  echo "ERROR: Swagger not reachable (HTTP ${SWAGGER_STATUS})."
  echo "       Start backend: dotnet run --project backend/src/AcadPrep.WebUI/AcadPrep.WebUI.csproj"
  exit 1
fi
echo "    OK - Swagger JSON reachable"
echo

echo "==> [2/4] GET /api/Exams/admin (before create)..."
curl -s "${BASE_URL}/api/Exams/admin" | python3 -m json.tool | head -40
echo

echo "==> [3/4] POST /api/Exams (create exam)..."
CREATE_RESPONSE="$(curl -s -w "\n%{http_code}" -X POST "${BASE_URL}/api/Exams" \
  -H "Content-Type: application/json" \
  -d "{
    \"title\": \"${EXAM_TITLE}\",
    \"description\": \"Created by test-swagger-create-exam.sh\",
    \"duration\": 120,
    \"examSeriesId\": ${EXAM_SERIES_ID}
  }")"

HTTP_BODY="$(echo "${CREATE_RESPONSE}" | head -n -1)"
HTTP_CODE="$(echo "${CREATE_RESPONSE}" | tail -n 1)"

echo "    HTTP Status: ${HTTP_CODE}"
echo "${HTTP_BODY}" | python3 -m json.tool

if [[ "${HTTP_CODE}" != "201" ]]; then
  echo
  echo "ERROR: Expected HTTP 201 Created."
  echo "       Tip: set EXAM_SERIES_ID to a valid ID from your database (seed default is often 1)."
  exit 1
fi

EXAM_ID="$(echo "${HTTP_BODY}" | python3 -c "import sys, json; print(json.load(sys.stdin).get('data', ''))")"
echo
echo "    Created Exam ID: ${EXAM_ID}"
echo

echo "==> [4/4] GET /api/Exams/admin/{id} (verify)..."
if [[ -n "${EXAM_ID}" && "${EXAM_ID}" != "None" ]]; then
  curl -s "${BASE_URL}/api/Exams/admin/${EXAM_ID}" | python3 -m json.tool | head -50
else
  echo "    Skipped - could not parse exam ID from response"
fi

echo
echo "==> Done. Open Swagger UI: ${BASE_URL}/swagger"
echo "    Try POST /api/Exams manually with the same JSON body."
