#!/bin/bash
set -e

DB_HOST="${DB_HOST:-db}"
DB_USER="${DB_USER:-postgres}"

echo "Waiting for database..."
until pg_isready -h "${DB_HOST}" -p 5432 -U "${DB_USER}" >/dev/null 2>&1; do
  echo "Database is unavailable - sleeping"
  sleep 2
done

echo "Starting application..."
cd /app
exec dotnet ElohimShop.API.dll
