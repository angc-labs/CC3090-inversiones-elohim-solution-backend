# Backend — Esmira Shop

API ASP.NET Core 10 + PostgreSQL. Arquitectura en capas (`Domain` → `Application` → `Infrastructure` → `API`).

## Requisitos

- .NET SDK 10
- Docker Desktop (recomendado)

## Levantar con Docker (recomendado)

Desde la raíz del monorepo:

```bash
docker compose up -d --build
```

- API: http://localhost:5000  
- Swagger: http://localhost:5000/swagger  
- Postgres: puerto host `5433`

Variables útiles en `.env` / `docker-compose.yml`:

| Variable | Descripción |
|----------|-------------|
| `SEED_DATA` | `true` en `backend/.env` carga `DemoDataSeeder` (productos, ventas, reservaciones, usuarios demo) |
| `SEED_DEMO_DATA` | Alias de `SEED_DATA` (compatibilidad) |
| `SUPER_ADMIN_EMAIL` / `SUPER_ADMIN_PASSWORD` | Super administrador inicial |

## Esquema de base de datos

**Fuente de verdad:** `../db/elohim_db.sql`  
**Parches idempotentes:** `../db/patch/`

El contenedor backend aplica SQL en el arranque (`entrypoint.sh`), no migraciones EF.

Para base nueva con Docker:

```bash
docker compose down -v
docker volume rm elohim_postgres_data   # si existe
docker compose up -d --build
```

## Desarrollo local (sin Docker)

```bash
cd backend
dotnet restore ElohimShop.slnx
dotnet build ElohimShop.slnx
dotnet run --project src/ElohimShop.API/ElohimShop.API.csproj
```

Conexión ejemplo (Postgres en Docker solo DB):

```bash
$env:ConnectionStrings__DefaultConnection='Host=localhost;Port=5433;Database=elohim;Username=postgres;Password=postgres'
$env:SEED_DATA='true'
dotnet run --project src/ElohimShop.API/ElohimShop.API.csproj
```

## Documentación API

- **[docs/endpoints.md](docs/endpoints.md)** — documentación de la API, auth, admin, seeds (`SEED_DATA`)
- **Swagger** — http://localhost:5000/swagger (Development)
- **Bruno** — `bruno/` colección de requests
- **Frontend rutas** — `../frontend/docs/RUTAS.md`

## EF Core migrations (opcional)

Las migraciones en `src/ElohimShop.Infrastructure/Migrations/` son legado/local.  
Si el modelo cambia, alinea primero `db/elohim_db.sql` y los `*Configuration.cs` (columnas `snake_case`).  
No ejecutes `ef database update` en Docker si el esquema ya viene del SQL.

## Más convenciones

Ver [AGENTS.md](AGENTS.md).
