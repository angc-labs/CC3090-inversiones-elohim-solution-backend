# Entity Framework Core - Plataforma DM Hub

La base de datos ahora se modela directamente con EF Core en la capa de Infrastructure. El arranque del contenedor ya no aplica `db/elohim_db.sql`; en su lugar, `PlatformDbContext` crea el esquema desde el modelo al iniciar la API.

## Estado actual

- `PlatformDbContext` es la fuente de verdad para el nuevo esquema multi-tenant.
- Los datos operativos llevan `tienda_id` y se filtran con `ITenantProvider`.
- Las tablas de autenticación estilo Better Auth están mapeadas en PostgreSQL con nombres exactos: `user`, `session`, `account` y `verification`.
- Docker levanta una base limpia con el volumen `dmhub_postgres_data`.

## Convenciones aplicadas

- Propiedades C# en PascalCase, columnas en el nombre exacto del contrato SQL.
- `jsonb` para `ConfiguracionVisual`.
- `NUMERIC(18,2)` para importes.
- `timestamp with time zone` para fechas.
- `HasQueryFilter` para entidades dependientes de tenant.

## Configuración de conexión

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db;Port=5432;Database=dmhub;Username=postgres;Password=postgres"
  }
}
```

## Arranque

`Program.cs` registra `PlatformDbContext`, `ITenantProvider` y `IPlatformService`, luego invoca `PlatformDatabaseBootstrapper.EnsureCreatedAsync(...)` para inicializar la BD si aún no existe.

## Validación

```bash
cd backend
dotnet build ElohimShop.slnx
docker compose up --build
```

## Notas

- `db/elohim_db.sql` queda como referencia histórica.
- Las migraciones existentes del esquema legado no son parte del nuevo flujo de arranque.
