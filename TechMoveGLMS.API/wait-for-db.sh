#!/usr/bin/env bash
set -e

# Default database host and port used in docker-compose. Can be overridden via env vars.
DB_HOST=${DB_HOST:-st10398576-techmoveglms-sql-server}
DB_PORT=${DB_PORT:-1433}

echo "Waiting for database ${DB_HOST}:${DB_PORT}..."

# Wait until the TCP port is open
while ! bash -c "</dev/tcp/${DB_HOST}/${DB_PORT}" >/dev/null 2>&1; do
  sleep 1
done

echo "Database is available — starting the application"

exec dotnet ST10398576_TechMoveGLMS.API.dll
