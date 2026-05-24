#!/bin/bash
set -e

DB_HOST="${DB_HOST:-db}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-postgres}"
DB_NAME="${DB_NAME:-elohim}"
export PGPASSWORD="${DB_PASSWORD}"

echo "Waiting for database..."
until pg_isready -h "${DB_HOST}" -p 5432 -U "${DB_USER}" >/dev/null 2>&1; do
  echo "Database is unavailable - sleeping"
  sleep 2
done

echo "Database is up - verifying schema (SQL, sin EF migrations)..."

TABLE_EXISTS="$(psql -h "${DB_HOST}" -U "${DB_USER}" -d "${DB_NAME}" -tAc \
  "SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Usuario' LIMIT 1" \
  2>/dev/null || true)"

if [ "${TABLE_EXISTS}" != "1" ]; then
  if [ -f /db/elohim_db.sql ]; then
    echo "Applying schema from /db/elohim_db.sql..."
    psql -h "${DB_HOST}" -U "${DB_USER}" -d "${DB_NAME}" -v ON_ERROR_STOP=1 -f /db/elohim_db.sql
  else
    echo "WARN: tabla Usuario no existe y /db/elohim_db.sql no está montado."
  fi
fi

if [ -d /db/patch ]; then
  for patch in /db/patch/*.sql; do
    if [ -f "${patch}" ]; then
      echo "Applying patch ${patch}..."
      psql -h "${DB_HOST}" -U "${DB_USER}" -d "${DB_NAME}" -v ON_ERROR_STOP=1 -f "${patch}"
    fi
  done
fi

echo "Starting application..."
cd /app
exec dotnet ElohimShop.API.dll
