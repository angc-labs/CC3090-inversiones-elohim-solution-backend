# Backend

## Requisitos
- .NET SDK 10
- Docker Desktop
- Proyecto ubicado en `backend/`

## 1) Levantar PostgreSQL
Ejecuta estos comandos desde la raiz del repo

```bash
docker compose down -v
docker compose up -d db
```

Esto elimina volumenes previos y crea una base nueva.

## 2) Entrar a backend
```bash
cd backend
```

## 3) Restaurar y compilar
```bash
dotnet restore ElohimShop.slnx
dotnet build ElohimShop.slnx
```

## 4) Instalar EF CLI
```bash
dotnet tool install --global dotnet-ef
```

Si ya está instalada:
```bash
dotnet tool update --global dotnet-ef
```

## 5) Aplicar migraciones existentes a una base vacia
```bash
$env:ConnectionStrings__DefaultConnection='Host=localhost;Port=5433;Database=elohim;Username=postgres;Password=postgres'; dotnet ef database update --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API
```

## 6) Verificar estado de migraciones
```bash
dotnet ef migrations list --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API
```

## 7) Ejecutar API
```bash
dotnet run --project src/ElohimShop.API/ElohimShop.API.csproj
```

Swagger (Development):
- http://localhost:5000/swagger


## Crear una nueva migracion
```bash
dotnet ef migrations add NombreDescriptivo --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API
```

Aplicar la migración:
```bash
$env:ConnectionStrings__DefaultConnection='Host=localhost;Port=5433;Database=elohim;Username=postgres;Password=postgres'; dotnet ef database update --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API
```
