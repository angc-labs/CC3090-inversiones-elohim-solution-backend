#!/bin/bash
set -e

echo "Waiting for database..."
until pg_isready -h ${DB_HOST:-db} -p 5432; do
  echo "Database is unavailable - sleeping"
  sleep 2
done

echo "Database is up - restoring packages and applying migrations..."
export ConnectionStrings__DefaultConnection="Host=${DB_HOST:-db};Port=5432;Database=${DB_NAME:-elohim};Username=${DB_USER:-postgres};Password=${DB_PASSWORD}"
cd /src
dotnet restore "src/ElohimShop.API/ElohimShop.API.csproj"
dotnet ef database update --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API

echo "Starting application..."
cd /app
exec dotnet ElohimShop.API.dll
